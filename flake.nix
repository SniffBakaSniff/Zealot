{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    utils.url = "github:numtide/flake-utils";
  };

  outputs =
    {
      self,
      nixpkgs,
      utils,
    }:
    utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs { inherit system; };
      in
      {
        devShell =
          with pkgs;
          mkShell {
            packages = [
	      # dotnet-aspnetcore_9
	      # clang
              dotnet-sdk_9
	      stdenv.cc.cc.lib
            ];
	    LD_LIBRARY_PATH = "${pkgs.stdenv.cc.cc.lib}/lib";
	    DOTNET_ROOT = "${pkgs.dotnet-sdk}/share/dotnet";
          };
      }
    );
}
