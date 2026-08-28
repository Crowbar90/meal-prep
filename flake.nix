{
  description = "MealPrepPlanner development shell";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

  outputs =
    { self, nixpkgs }:
    let
      systems = [
        "x86_64-linux"
        "aarch64-linux"
        "x86_64-darwin"
        "aarch64-darwin"
      ];
      forEach = nixpkgs.lib.genAttrs systems;
    in
    {
      devShells = forEach (system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
          dotnetSdk = pkgs.dotnet-sdk_10;
        in
        {
          default = pkgs.mkShell {
            packages = [
              dotnetSdk
              pkgs.podman
            ];

            # The .NET test runner and the Aspire AppHost are native apphosts
            # that locate the runtime via DOTNET_ROOT; on NixOS the SDK lives
            # in /nix/store, so the symlink chain alone isn't enough.
            env = {
              DOTNET_ROOT = "${dotnetSdk}";
            };
          };
        });
    };
}
