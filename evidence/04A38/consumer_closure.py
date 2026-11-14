"""Independent compiled Windows-to-Core/Common consumer edge inventory."""
import os
import subprocess
import sys
from run import ROOT, HERE, PRIVATE, m


def main():
    config, attempt = sys.argv[1:3]
    tag = 'consumer-closure-' + config + '-' + attempt
    folder = PRIVATE / tag
    folder.mkdir(exist_ok=False)
    locator = os.path.join(os.environ['ProgramFiles(x86)'], 'Microsoft Visual Studio/Installer/vswhere.exe')
    compiler = subprocess.check_output([locator, '-products', '*', '-version', '[18.0,19.0)', '-find', 'MSBuild/Current/Bin/Roslyn/csc.exe'], text=True).splitlines()[0]
    runner = m.Runner(PRIVATE)
    result = runner.command(tag + '-compile', [compiler, '/nologo', '/target:exe', '/reference:System.Core.dll', '/reference:System.Web.Extensions.dll', '/out:' + str(folder / 'ConsumerClosure38.exe'), str(HERE / 'helpers/ConsumerClosure38.cs')])
    assert result['exit_code'] == 0
    result = runner.command(tag + '-read', [str(folder / 'ConsumerClosure38.exe'), str(PRIVATE / 'builds/entry' / config / 'legacy'), str(HERE / 'entry-members.json'), str(HERE / ('preedit/consumer-closure-' + config + '-' + attempt + '.json'))])
    assert result['exit_code'] == 0


if __name__ == '__main__':
    main()
