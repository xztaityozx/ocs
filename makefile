PREFIX ?= ${HOME}/.local/bin
publish:
	dotnet publish -c Release ./ocs/ocs.csproj

install:
	echo ln -s ${PWD}/ocs/bin/Release/net6.0/ocs ${PREFIX:/=}/ocs

uninstall:
	unlink ${PREFIX}/ocs
