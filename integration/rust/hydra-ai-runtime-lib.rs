use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub enum ModelProvider {
    Ollama,
    OpenAi,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub enum DataClassification {
    Public,
    Internal,
    PrivateLocal,
    Secret,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AiToolRequest {
    pub call_id: String,
    pub name: String,
    pub arguments_json: String,
    pub requires_approval: bool,
}

#[derive(Debug, thiserror::Error)]
pub enum HydraAiError {
    #[error("provider unavailable: {0}")]
    ProviderUnavailable(String),
    #[error("policy denied request: {0}")]
    PolicyDenied(String),
    #[error("invalid tool request: {0}")]
    InvalidToolRequest(String),
}
