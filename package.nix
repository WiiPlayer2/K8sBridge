{ lib
, buildDotnetModule

, dotnet-sdk
, kubectl
, frp
}:
with lib;
buildDotnetModule rec {
  pname = "k8s-bridge";
  version = "0.1";

  src = fileset.toSource {
    root = ./.;
    fileset = fileset.fromSource (sources.cleanSource ./.);
  };

  projectFile = "K8sBridge/K8sBridge.csproj";
  nugetDeps = ./deps.json; # nix build .#k8s-bridge.passthru.fetch-deps
  dotnetInstallFlags = "-f net8.0";

  makeWrapperArgs = [
    "--prefix"
    "LD_LIBRARY_PATH"
    ":"
    "${dotnet-sdk.icu}/lib"
    "--suffix"
    "PATH"
    ":"
    (lib.makeBinPath [
      kubectl
      frp
    ])
  ];

  meta = with lib; {
    homepage = "https://github.com/WiiPlayer2/K8sBridge";
    description = "A tool to bridge traffic from a kubernetes cluster to local apps";
    license = licenses.mit;
  };
}
