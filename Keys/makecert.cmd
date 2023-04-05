REM Запускать в среде Developer Command Prompt for vs 2022
REM Создание и установка с приватным ключом доступным для экcпорта:
makecert -r -sv "D:\Projects\RMGeo\Keys\RmSolution.RmGeo.pvk" -pe -n CN=RmSolution.RmGeo -sr LocalMachine -ss root -# 16121972 -$ commercial -a sha256 -e 12/31/2049 -l www.lesev.ru -sky exchange "D:\Projects\RMGeo\Keys\RmSolution.RmGeo.cer"

REM Создание сертификата с включённым приватным ключом и паролем:
pvk2pfx -pvk "D:\Projects\RMGeo\Keys\RmSolution.RmGeo.pvk" -spc "D:\Projects\RMGeo\Keys\RmSolution.RmGeo.cer" -pfx "D:\Projects\RMGeo\Keys\RmSolution.RmGeo.pfx" -pi 7HfpJnvthm!

REM Создание SSL Сертификата:
makecert -pe -n CN=RmSolution.RmGeo -a sha1 -sky exchange -eku 1.3.6.1.5.5.7.3.1 -ic LesevCA.cer -iv LesevCA.pvk -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12 -sv "D:\Projects\RMGeo\Keys\RmSolution.RmGeo.pvk" "D:\Projects\RMGeo\Keys\RmSolution.RmGeo.cer"

REM Создание сертификата SSL, phrase: 7HfpJnvthm!
openssl req -new -x509 -newkey rsa:2048 -keyout RmSolution.RmGeo.key -out RmSolution.RmGeo.cer -days 3650 -subj /CN=RmSolution.RmGeo
openssl pkcs12 -export -out RmSolution.RmGeo.pfx -inkey RmSolution.RmGeo.key -in RmSolution.RmGeo.cer
