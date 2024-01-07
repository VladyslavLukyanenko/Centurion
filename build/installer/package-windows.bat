@echo off

mkdir .\artifacts\cli

echo Publish x64 version
rmdir /Q/S .\bin\cli\win\x64
rmdir /Q/S .\bin\notifier\win\x64
cd ..\..\src\ui\Centurion.Cli
rmdir /Q/S .\obj
rmdir /Q/S .\bin
rem -p:PublishReadyToRun=true 
dotnet publish -c Release --self-contained -r win-x64 -p:PublishReadyToRun=true --framework net6.0-windows -o ..\..\..\build\installer\bin\cli\win\x64 -v q --nologo .


echo Publish WindowsNotifier x64 version
cd ..\Centurion.Cli.Util.WindowsNotifications
dotnet publish -c Release -o ..\..\..\build\installer\bin\notifier\win\x64 -p:PublishTrimmed=true --self-contained=true -p:TrimMode=link -r win-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true -v q --nologo .

echo Copying deps
cd ..\..\..\build\installer\
cp .\bin\notifier\win\x64\Centurion-Notifications.exe .\bin\cli\win\x64\
cp .\win-x64\av_libglesv2.dll .\bin\cli\win\x64\
mkdir .\bin\cli\win\x64\external\
cp ..\..\..\centurion-3rdparty-deps\chrome.zip .\bin\cli\win\x64\external\

echo Creating x64 installer
iscc /Qp /Darch=x64 .\installer.iss

echo Done
echo on

pause
