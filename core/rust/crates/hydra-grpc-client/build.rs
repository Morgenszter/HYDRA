fn main() -> Result<(), Box<dyn std::error::Error>> {
  let protoc = protoc_bin_vendored::protoc_bin_path()?;
  let mut prost_config = prost_build::Config::new();
  prost_config.protoc_executable(protoc);

  tonic_build::configure()
    .build_server(false)
    .compile_protos_with_config(
      prost_config,
      &["../../../../contracts/proto/hydra.runtime.v1.proto"],
      &["../../../../contracts/proto"],
    )?;

  println!("cargo:rerun-if-changed=../../../../contracts/proto/hydra.runtime.v1.proto");
  Ok(())
}