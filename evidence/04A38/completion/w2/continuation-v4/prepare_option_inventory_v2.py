from common import *
s=(HERE/'options_inventory.py').read_text(encoding='utf-8');needle="name=h['name']\n";assert needle in s
s=s.replace(needle,needle+"  if path.endswith('UdfDataImportAgreement.vb') and name not in ['ParseAgreementCiMethod','ParseBlandAltmanMode','ParseBlandAltmanScale','ParseBlandAltmanXAxisMode','ParseBlandAltmanPlotMode','ParseKappaWeighting','ParseDemingVarianceModel','ParseIccModel']:continue\n")
with (HERE/'options_inventory_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/options-inventory-v1.json',dict(status='FAILED_READER_SETUP_RETAINED',reason='Broad Parse prefix included unrelated ParseAlphaOrDefault not in the reviewed30method cohort; no proof or packet output was written',correction='Explicitly limit agreement cohort to the8 reviewed identities; all other helpers remain OPEN'))
