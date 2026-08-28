# Inventory Service Configuration

StackPulse uses one AWS Secrets Manager secret for the whole application. The API and inventory service both read this secret through the AWS SDK default credential chain.

## Secret Shape

Store the secret value as JSON:

```json
{
  "mysqlConnectionString": "server=mysql-host;database=stackpulse_master;user=stackpulse;password=change-me;",
  "mongoConnectionString": "mongodb+srv://user:password@cluster/stackpulse_transactions"
}
```

Then configure both the API and inventory service:

```json
{
  "AwsSecretsManager": {
    "Enabled": true,
    "Region": "us-east-1",
    "SecretName": "stackpulse"
  }
}
```

## AWS EC2

Use an EC2 instance profile / IAM role. The role needs `secretsmanager:GetSecretValue` permission for the single StackPulse secret. No access keys should be stored on the machine.

## Datacenter / On-Premise

The same code works outside AWS because the AWS SDK can read credentials from the standard credential chain:

- Environment variables: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, and optionally `AWS_SESSION_TOKEN`.
- Shared credentials file, usually `~/.aws/credentials` on Linux/macOS or `%USERPROFILE%\.aws\credentials` on Windows.
- A named profile selected with `AWS_PROFILE`.

For production datacenter servers, prefer short-lived credentials from your identity provider or a secure credential rotation process. If AWS Secrets Manager cannot be reached from the datacenter, keep `AwsSecretsManager:Enabled` as `false` and inject `DatabaseSettings:ConnectionString` and `MongoDbSettings:ConnectionString` through the server's local secret store, environment variables, or deployment pipeline.

## Supported Inventory Hosts

The inventory worker now runs as:

- Windows Service on Windows.
- systemd service on Linux.
- Console/launchd-managed process on macOS.

Windows collects WMI services, registry software, and drives. Linux collects systemd services, dpkg package metadata when available, and drives. macOS collects launchd services, application bundles, and drives.
