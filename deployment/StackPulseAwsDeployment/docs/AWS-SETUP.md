# One-Time AWS Setup

1. Create a VPC and private application subnets across at least two AZs.
2. Create ALB in public subnets.
3. Create ACM certificate and HTTPS listener.
4. Create application SG: app port only from ALB SG.
5. Create SQL SG: SQL port only from application SG.
6. Create Jenkins SG restricted to trusted administration sources.
7. Create application EC2 IAM instance profile for SSM/Secrets Manager.
8. Create S3 build artifact bucket and builder IAM role.
9. Build one immutable AMI per application.
10. Put each AMI ID in config/production.json.
11. Run Java deploy command for each application.
12. Create DNS records pointing to the ALB DNS name.
13. Configure AWS Budgets and CloudWatch.
14. Test health checks and rollback.

Avoid public SSH when possible; use Systems Manager Session Manager.
