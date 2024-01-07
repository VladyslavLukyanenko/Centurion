#!/bin/bash

MODE=Release
ROOT_DIR="$(pwd)/../.."
PROJECT_DIR="$(pwd)/../../src/ui/Centurion.Cli"

echo "Preparation"
mkdir -p ./artifacts/aio
rm -rv $PROJECT_DIR/bin/$MODE
rm -rv $PROJECT_DIR/obj/$MODE
rm -rv $ROOT_DIR/build/installer/bin/aio/osx/x64
rm -rv $ROOT_DIR/build/installer/artifacts/aio/Centurion-AIO.app

echo "Publishing"
dotnet publish -c $MODE --self-contained --framework net6.0 -r osx-x64 -o $ROOT_DIR/build/installer/bin/aio/osx/x64 -v q --nologo $PROJECT_DIR
cp $ROOT_DIR/../centurion-3rdparty-deps/chrome.zip ./bin/aio/osx/x64/external/

echo "Bundling app"
cd $PROJECT_DIR && dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:TargetFramework=net6.0 -property:Configuration=$MODE -p:UseAppHost=true
mkdir $PROJECT_DIR/bin/$MODE/net6.0/osx-x64/publish/Centurion-AIO.app/Contents/MacOS/external/
cp $ROOT_DIR/../centurion-3rdparty-deps/chrome.zip $PROJECT_DIR/bin/$MODE/net6.0/osx-x64/publish/Centurion-AIO.app/Contents/MacOS/external/

echo "Moving packages app to final dir"
mv $PROJECT_DIR/bin/$MODE/net6.0/osx-x64/publish/Centurion-AIO.app $ROOT_DIR/build/installer/artifacts/aio/