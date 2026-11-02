"""Read-only local NTFS directory identity; no string-prefix trust or path execution.

Only ordinary absolute C: DOS paths are accepted. Slash/case/real 8.3 aliases
must resolve through non-reparse, deny-delete-held directory chains to the same
volume/file ID as the expected pinned directory. No UNC/device-prefix fallback.
"""
from contextlib import contextmanager, ExitStack
import ctypes
from ctypes import wintypes
import os
import re


class PathIdentityError(ValueError):
    pass


def require(condition, message):
    if not condition:
        raise PathIdentityError(message)


def literal_path(value):
    require(isinstance(value, str) and 3 < len(value) <= 1024, 'Path must be a bounded string')
    require(re.match(r'^[cC]:[\\/]', value) is not None, 'Only absolute local C: DOS paths are admitted')
    # Validate original spelling before Windows/Python can strip dots or collapse ..
    parts = re.split(r'[\\/]', value[3:])
    reserved = {'CON', 'PRN', 'AUX', 'NUL', *(f'COM{i}' for i in range(1, 10)), *(f'LPT{i}' for i in range(1, 10))}
    for part in parts:
        require(part not in ('', '.', '..') and not part.endswith((' ', '.')), 'Empty/traversal/alias component refused')
        require(not any(ord(c) < 32 or c in ':<>"|?*' for c in part), 'Control/device/ADS/wildcard syntax refused')
        require(part.split('.')[0].upper() not in reserved, 'Windows device component refused')
    return 'C:\\' + '\\'.join(parts)


class FileInformation(ctypes.Structure):
    _fields_ = [('attributes', wintypes.DWORD),
                ('creation', wintypes.FILETIME), ('access', wintypes.FILETIME), ('write', wintypes.FILETIME),
                ('volume', wintypes.DWORD), ('size_high', wintypes.DWORD), ('size_low', wintypes.DWORD),
                ('links', wintypes.DWORD), ('index_high', wintypes.DWORD), ('index_low', wintypes.DWORD)]


class WindowsApi:
    def __init__(self):
        require(os.name == 'nt', 'Windows identity API required; no fallback')
        require(ctypes.sizeof(FileInformation) == 52, 'Unexpected Windows structure layout')
        kernel = ctypes.WinDLL('kernel32', use_last_error=True)
        self.create = kernel.CreateFileW
        self.create.argtypes = [wintypes.LPCWSTR, wintypes.DWORD, wintypes.DWORD, wintypes.LPVOID,
                                wintypes.DWORD, wintypes.DWORD, wintypes.HANDLE]
        self.create.restype = wintypes.HANDLE
        self.close = kernel.CloseHandle
        self.close.argtypes = [wintypes.HANDLE]
        self.close.restype = wintypes.BOOL
        self.info = kernel.GetFileInformationByHandle
        self.info.argtypes = [wintypes.HANDLE, ctypes.POINTER(FileInformation)]
        self.info.restype = wintypes.BOOL
        self.final = kernel.GetFinalPathNameByHandleW
        self.final.argtypes = [wintypes.HANDLE, wintypes.LPWSTR, wintypes.DWORD, wintypes.DWORD]
        self.final.restype = wintypes.DWORD
        self.volume = kernel.GetVolumeInformationW
        self.volume.argtypes = [wintypes.LPCWSTR, wintypes.LPWSTR, wintypes.DWORD,
                                ctypes.POINTER(wintypes.DWORD), ctypes.POINTER(wintypes.DWORD),
                                ctypes.POINTER(wintypes.DWORD), wintypes.LPWSTR, wintypes.DWORD]
        self.volume.restype = wintypes.BOOL
        self.drive_type = kernel.GetDriveTypeW
        self.drive_type.argtypes = [wintypes.LPCWSTR]
        self.drive_type.restype = wintypes.UINT

    def local_ntfs(self):
        require(self.drive_type('C:\\') == 3, 'C: must be a local fixed drive')
        filesystem = ctypes.create_unicode_buffer(32)
        serial = wintypes.DWORD()
        if not self.volume('C:\\', None, 0, ctypes.byref(serial), None, None, filesystem, len(filesystem)):
            raise ctypes.WinError(ctypes.get_last_error())
        require(filesystem.value == 'NTFS', 'Only qualified NTFS 64-bit file identity is admitted')
        return serial.value

    def close_checked(self, handle):
        if not self.close(handle):
            raise ctypes.WinError(ctypes.get_last_error())

    def open_directory(self, path, stack, volume):
        # FILE_LIST_DIRECTORY|FILE_READ_ATTRIBUTES; SHARE_READ|WRITE, never DELETE.
        # OPEN_EXISTING, BACKUP_SEMANTICS|OPEN_REPARSE_POINT: inspect links themselves.
        handle = self.create(path, 0x81, 3, None, 3, 0x02000000 | 0x00200000, None)
        if handle == wintypes.HANDLE(-1).value:
            raise ctypes.WinError(ctypes.get_last_error())
        stack.callback(self.close_checked, handle)
        info = FileInformation()
        if not self.info(handle, ctypes.byref(info)):
            raise ctypes.WinError(ctypes.get_last_error())
        require(bool(info.attributes & 0x10), 'Non-directory object refused')
        require(not info.attributes & 0x400, 'Reparse point/junction/symlink refused')
        require(info.volume == volume, 'Directory crosses the qualified volume')
        buffer = ctypes.create_unicode_buffer(32768)
        length = self.final(handle, buffer, len(buffer), 0)  # normalized name, DOS volume
        if not length:
            raise ctypes.WinError(ctypes.get_last_error())
        require(length < len(buffer), 'Canonical path truncated')
        require(buffer.value.startswith('\\\\?\\C:\\'), 'Unexpected canonical volume/path format')
        index = (info.index_high << 32) | info.index_low
        require(index != 0, 'Missing stable NTFS file identity')
        return dict(opened_path=path, canonical_path=buffer.value, filesystem='NTFS',
                    volume_serial=info.volume, file_index=index, attributes=info.attributes)


def open_chain(api, path, stack, volume):
    rows = [api.open_directory('C:\\', stack, volume)]
    cursor = 'C:'
    for part in path[3:].split('\\'):
        cursor += '\\' + part
        rows.append(api.open_directory(cursor, stack, volume))
    return rows


@contextmanager
def same_trusted_directory(observed, expected, pinned_identity=None):
    """Hold both full chains through the caller's validation; close only owned handles.

    Qualification may omit pinned_identity to collect the initial trusted ID.
    Production v6 always supplies its manifest-pinned volume/index pair.
    """
    observed_path, expected_path = literal_path(observed), literal_path(expected)
    try:
        api = WindowsApi()
        volume = api.local_ntfs()
        with ExitStack() as stack:
            trusted = open_chain(api, expected_path, stack, volume)
            actual = open_chain(api, observed_path, stack, volume)
            identity = {key: trusted[-1][key] for key in ('filesystem', 'volume_serial', 'file_index')}
            if pinned_identity is not None:
                require(identity == pinned_identity, 'Trusted directory identity changed from the pinned policy')
            require(all(actual[-1][key] == identity[key] for key in identity), 'Observed path is not the same trusted directory')
            yield dict(observed=observed, expected=expected, identity=identity,
                       observed_canonical=actual[-1]['canonical_path'], expected_canonical=trusted[-1]['canonical_path'],
                       observed_chain=actual, expected_chain=trusted, held_deny_delete=True)
    except OSError as error:
        raise PathIdentityError(f'Windows directory identity query failed: {error}') from error
