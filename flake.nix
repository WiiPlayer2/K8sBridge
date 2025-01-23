{
  description = "A tool to bridge traffic from a kubernetes cluster to local apps";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs?ref=nixos-unstable";
    flake-parts.url = "github:hercules-ci/flake-parts";
  };

  outputs = { flake-parts, ... } @ inputs:
    flake-parts.lib.mkFlake
    { inherit inputs; }
    (
      {
        systems = [
          "x86_64-linux"
          "aarch64-linux"
          "i686-linux"
          "x86_64-darwin"
          "aarch64-darwin"
        ];

        flake = {
          overlays.default =
            final: prev:
            {
              k8s-bridge = final.pkgs.callPackage ./package.nix {};
            };
        };

        perSystem =
          { pkgs, ... }:
          {
            packages = rec {
              k8s-bridge = pkgs.callPackage ./package.nix {};
              default = k8s-bridge;
            };
            
            devShells = rec {
              default = publishing;
              publishing = pkgs.mkShell {
                packages = with pkgs; [
                  dotnet-sdk_8
                  dotnetPackages.Nuget
                  
                  (writeShellScriptBin "publish-to-nuget" ''
                    set -xe
                    dotnet pack ./K8sBridge/K8sBridge.csproj -o packages --version-suffix pre$(date -u +%+4Y%m%d%H%m)
                    trap "rm ./packages/k8s-bridge.*.nupkg" EXIT
                    dotnet nuget push ./packages/k8s-bridge.*.nupkg --source nuget.org --skip-duplicate "$@"
                  '')
                ];
              };
            };            
          };
      }
    );
}
