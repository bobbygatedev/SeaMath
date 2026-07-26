copy VC_redist.x64.exe SeaMath
mkdir tmp
"%programfiles%\7-Zip\7z.exe" x "ucrt64.7z" -o"tmp" -y
mkdir Seamath\PlugIns\SeaMathGatePadPlugin\gcc\x64
xcopy /Y /S tmp\ucrt64\* Seamath\PlugIns\SeaMathGatePadPlugin\gcc\x64
rmdir /s /q tmp