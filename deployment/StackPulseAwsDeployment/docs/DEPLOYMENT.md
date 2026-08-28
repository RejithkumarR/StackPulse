# Deployment

Build:
```bash
mvn -B clean package
```

Deploy StackPulse:
```bash
java -jar target/stackpulse-aws-deployment-1.0.0.jar deploy stackpulse --config config/production.json
```

Deploy second application:
```bash
java -jar target/stackpulse-aws-deployment-1.0.0.jar deploy second-app --config config/production.json
```

Release flow:
Source -> Jenkins -> Java build/test -> Docker image -> builder EC2 -> AMI -> Launch Template -> ASG -> ALB.

For production, use a new Launch Template version and an ASG instance refresh for rolling replacement. Keep the previous AMI available for rollback.
