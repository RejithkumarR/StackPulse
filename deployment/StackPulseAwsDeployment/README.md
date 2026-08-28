# StackPulse AWS Deployment - Java

Java 21 + Maven AWS deployment utility for the requested EC2 + Docker + AMI + ALB + Auto Scaling architecture.

## Shared infrastructure
One VPC, one ALB, one SQL Server Linux EC2, one Jenkins EC2, shared S3/certificates/DNS.

## Separate applications
Each application gets its own AMI, Launch Template, Target Group, ASG and configuration/secrets namespace.

## Build
```bash
cp config/production.example.json config/production.json
# edit placeholders
mvn -B clean package
```

## Deploy
```bash
java -jar target/stackpulse-aws-deployment-1.0.0.jar deploy stackpulse --config config/production.json
java -jar target/stackpulse-aws-deployment-1.0.0.jar deploy second-app --config config/production.json
```

The packaging stage is deliberately builder-oriented: Jenkins creates the compiled application/Docker image and the dedicated builder creates the immutable AMI. The deployment stage consumes the AMI ID.

## Business hours
The example schedule is Monday-Saturday, 08:00-20:00 Asia/Kolkata, represented in UTC as 02:30 and 14:30. Verify the recurrence against your AWS scheduling implementation before production.

## Important
The ZIP contains the Java deployment/orchestration source and sample container definitions. Your actual StackPulse application source is not fabricated; place the real source/build context under the configured sourcePath.


## CI build

Use Java 21:

```bash
mvn -B clean test
mvn -B package
```

The GitHub Actions Java job should use:

```yaml
- uses: actions/setup-java@v4
  with:
    distribution: temurin
    java-version: '21'
    cache: maven

- run: mvn -B clean package
```

The AWS deployment code is not executed during ordinary pull-request CI.
