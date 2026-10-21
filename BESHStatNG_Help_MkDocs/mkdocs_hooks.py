"""Output packaging shared by direct MkDocs and owner-wrapper builds."""
from pathlib import Path
from urllib.parse import urlsplit

from mkdocs.exceptions import PluginError


MJS_HTACCESS = (
    "<IfModule mod_mime.c>\n"
    "    AddType text/javascript .mjs\n"
    "</IfModule>\n"
).encode("utf-8")


def on_post_build(config, **kwargs):
    """Write and verify the latest site's Apache module MIME configuration."""
    if not urlsplit(config["site_url"] or "").path.rstrip("/").endswith("/latest"):
        return

    output = Path(config["site_dir"]).resolve()
    artifact = output / "assets" / "javascripts" / ".htaccess"
    try:
        # Refuse an output symlink that would send this write outside site_dir.
        artifact.resolve().relative_to(output)
        artifact.parent.mkdir(parents=True, exist_ok=True)
        artifact.write_bytes(MJS_HTACCESS)
        if artifact.read_bytes() != MJS_HTACCESS:
            raise OSError("generated content differs from the required UTF-8/LF bytes")
    except (OSError, ValueError) as error:
        raise PluginError(f"Cannot generate latest Help MIME configuration at {artifact}: {error}") from error
