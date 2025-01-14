{ lib
, buildDotnetModule
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

  meta = with lib; {
    homepage = "https://github.com/WiiPlayer2/K8sBridge";
    description = "A tool to bridge traffic from a kubernetes cluster to local apps";
    license = licenses.mit;
  };
}
