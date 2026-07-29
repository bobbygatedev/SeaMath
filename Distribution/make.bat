@echo off
setlocal EnableDelayedExpansion

set "VS_PATH="

for /d %%D in ("C:\Program Files\Microsoft Visual Studio\2022\*") do (
    if exist "%%D\MSBuild\Current\Bin\MSBuild.exe" (
        set "VS_PATH=%%D"
        goto :found
    )
)

:found
if not defined VS_PATH (
    echo Visual Studio not found
    exit /b 1
)

echo Visual Studio trovato:
echo %VS_PATH%

set MSBUILD="%VS_PATH%\MSBuild\Current\Bin\amd64\MSBuild.exe"

echo MSBuild:
echo %MSBUILD%

set SAVE=%cd%
cd ..
git clean -fdx
dotnet nuget locals all --clear
%MSBUILD% SeaMath.sln /t:Restore
%MSBUILD% Scintilla\win32\SciLexer.vcxproj /t:Build /p:Configuration=Release /p:Platform=x64
%MSBUILD% GatePad\GatePad.csproj /t:Build /p:Configuration=Release /p:Platform=x64
%MSBUILD% GateDockRuntimePlugIn\GateDockRuntimePlugIn.csproj /t:Build /p:Configuration=Release /p:Platform=x64
%MSBUILD% SeaMathGatePadPlugin\SeaMathGatePadPlugin.csproj /t:Build /p:Configuration=Release /p:Platform=x64

rem %MSBUILD% SeaMath.sln /t:Build /p:Configuration=Release /p:Platform=x64

cd %SAVE%

mkdir SeaMath
mkdir SeaMath\PlugIns\SeaMathGatePadPlugin\dev
xcopy /Y /S ..\GatePad\bin\x64\Release\net8.0-windows\* SeaMath 
xcopy /Y /S ..\SeaMath\dev SeaMath\PlugIns\SeaMathGatePadPlugin\dev

copy VC_redist.x64.exe SeaMath
copy windowsdesktop-runtime-8.0.29-win-x64.exe SeaMath
mkdir tmp
"%programfiles%\7-Zip\7z.exe" x "ucrt64.7z" -o"tmp" -y
mkdir Seamath\PlugIns\SeaMathGatePadPlugin\gcc\x64
xcopy /Y /S tmp\ucrt64\* Seamath\PlugIns\SeaMathGatePadPlugin\gcc\x64
rmdir /s /q tmp

endlocal

