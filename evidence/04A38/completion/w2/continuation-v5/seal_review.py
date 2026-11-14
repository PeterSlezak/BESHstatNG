from common import *
path = HERE / 'verify_slice.py'
text = path.read_text(encoding='utf-8')
assert "REVIEW_SHA256 = 'REVIEW_NOT_SEALED'" in text
text = text.replace("REVIEW_SHA256 = 'REVIEW_NOT_SEALED'", "REVIEW_SHA256 = '" + sha((HERE / 'body-contracts.json').read_bytes()) + "'")
path.write_text(text, encoding='utf-8', newline='\n')
save('review-seal.json', dict(review=pin(HERE / 'body-contracts.json'), reader=pin(path), scope='Complete selected body review; no original semantic query'))
