use serde::{Deserialize, Serialize};
use std::time::SystemTime;

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub enum RuntimeHealth {
  Ready,
  Degraded,
  Critical,
  Offline,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RuntimeSubsystem {
  pub id: String,
  pub display_name: String,
  pub health: RuntimeHealth,
}

#[derive(Debug, Clone)]
pub struct RuntimeSnapshot {
  pub runtime_id: String,
  pub health: RuntimeHealth,
  pub subsystems: Vec<RuntimeSubsystem>,
  pub captured_at: SystemTime,
}

#[derive(Debug, thiserror::Error)]
pub enum HydraCoreError {
  #[error("runtime unavailable: {0}")]
  RuntimeUnavailable(String),
  #[error("invalid command: {0}")]
  InvalidCommand(String),
}

pub type HydraResult<T> = Result<T, HydraCoreError>;
