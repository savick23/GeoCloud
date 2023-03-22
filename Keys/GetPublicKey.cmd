@ECHO OFF
SET sn="C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\sn.exe"
%sn% -p RmSolution.snk RmSolution.PublicKey
%sn% -tp RmSolution.PublicKey
PAUSE