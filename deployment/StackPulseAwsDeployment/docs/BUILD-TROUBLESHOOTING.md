# Java build troubleshooting

The original build failed because EC2 Launch Template model classes were imported from the Auto Scaling package.

Correct AWS SDK v2 split:

- `RequestLaunchTemplateData`
- `CreateLaunchTemplateRequest`
- `CreateLaunchTemplateVersionRequest`
- `DescribeLaunchTemplatesRequest`
- `LaunchTemplateIamInstanceProfileSpecificationRequest`
- `LaunchTemplateBlockDeviceMappingRequest`
- `LaunchTemplateEbsBlockDeviceRequest`

come from:

`software.amazon.awssdk.services.ec2.model`

Auto Scaling policies and scheduled actions come from:

`software.amazon.awssdk.services.autoscaling.model`

For target tracking, the Auto Scaling API accepts:

`TargetTrackingScaling`

and the predefined CPU metric is:

`MetricType.ASG_AVERAGE_CPU_UTILIZATION`

AWS SDK for Java documentation confirms that EC2 owns the Launch Template request/data model and Auto Scaling exposes the target-tracking policy and predefined CPU metric. 

Run:

```bash
mvn -B clean test
mvn -B package
```

Do not run the deployment command in CI until AWS credentials, IAM permissions, networking, and production configuration are intentionally supplied.
