# StackPulse AWS Java Deployment

Shared: VPC, ALB, HTTPS listener/certificate, Linux SQL Server, Jenkins, S3.
Per application: immutable AMI, Launch Template, Target Group, ASG, secrets/parameters.

ERP/HRMS can share the SQL host while using separate databases.

ALB rules:
- stackpulse.example.com -> StackPulse target group
- second.example.com -> second-app target group

Do not put an ALB IP in DNS. Use the ALB DNS name / Route 53 Alias.

Scaling:
- Mon-Sat 08:00 IST -> desired/min 1
- Mon-Sat 20:00 IST -> desired/min 0
- CPU target can scale to max 2 during business hours.
