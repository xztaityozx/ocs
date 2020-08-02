#!/usr/bin/env bash

runtime_id="linux-x64"
[[ "${1}" = "macOS-latest" ]] && runtime_id="osx-x64"


cd ./ocs
dotnet publish -r ${runtime_id} -c Release --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o ${runtime_id}
