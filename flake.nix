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
          };
      }
    );
}
