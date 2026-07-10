use hydra_core::{HydraCoreError, HydraResult, RuntimeHealth, RuntimeSnapshot, RuntimeSubsystem};
use std::time::{Duration, SystemTime, UNIX_EPOCH};
use tonic::transport::Channel;

pub mod hydra {
  pub mod runtime {
    pub mod v1 {
      tonic::include_proto!("hydra.runtime.v1");
    }
  }
}

use hydra::runtime::v1::hydra_runtime_service_client::HydraRuntimeServiceClient;
use hydra::runtime::v1::{GetRuntimeSnapshotRequest, RuntimeHealth as ProtoRuntimeHealth};

pub struct HydraRuntimeGrpcClient {
  inner: HydraRuntimeServiceClient<Channel>,
}

impl HydraRuntimeGrpcClient {
  pub async fn connect(endpoint: String) -> HydraResult<Self> {
    let inner = HydraRuntimeServiceClient::connect(endpoint)
      .await
      .map_err(|error| HydraCoreError::RuntimeUnavailable(error.to_string()))?;

    Ok(Self { inner })
  }

  pub async fn get_snapshot(&mut self, client_id: String) -> HydraResult<RuntimeSnapshot> {
    let response = self
      .inner
      .get_runtime_snapshot(GetRuntimeSnapshotRequest { client_id })
      .await
      .map_err(|error| HydraCoreError::RuntimeUnavailable(error.to_string()))?
      .into_inner();

    let captured_at = response
      .captured_at
      .and_then(system_time_from_timestamp)
      .unwrap_or_else(SystemTime::now);

    let health = map_health(response.health());

    let subsystems = response
      .subsystems
      .into_iter()
      .map(|subsystem| {
        let health = map_health(subsystem.health());

        RuntimeSubsystem {
          id: subsystem.id,
          display_name: subsystem.display_name,
          health,
        }
      })
      .collect();

    Ok(RuntimeSnapshot {
      runtime_id: response.runtime_id,
      health,
      subsystems,
      captured_at,
    })
  }
}

fn map_health(value: ProtoRuntimeHealth) -> RuntimeHealth {
  match value {
    ProtoRuntimeHealth::Ready => RuntimeHealth::Ready,
    ProtoRuntimeHealth::Degraded => RuntimeHealth::Degraded,
    ProtoRuntimeHealth::Critical => RuntimeHealth::Critical,
    ProtoRuntimeHealth::Offline => RuntimeHealth::Offline,
    ProtoRuntimeHealth::Unspecified => RuntimeHealth::Degraded,
  }
}

fn system_time_from_timestamp(timestamp: prost_types::Timestamp) -> Option<SystemTime> {
  if timestamp.seconds < 0 || timestamp.nanos < 0 {
    return None;
  }

  let seconds = u64::try_from(timestamp.seconds).ok()?;
  let nanos = u32::try_from(timestamp.nanos).ok()?;

  UNIX_EPOCH.checked_add(Duration::new(seconds, nanos))
}
