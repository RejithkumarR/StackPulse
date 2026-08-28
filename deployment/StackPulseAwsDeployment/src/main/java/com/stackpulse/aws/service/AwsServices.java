package com.stackpulse.aws.service;
import software.amazon.awssdk.regions.Region;
import software.amazon.awssdk.services.ec2.Ec2Client;
import software.amazon.awssdk.services.autoscaling.AutoScalingClient;
import software.amazon.awssdk.services.elasticloadbalancingv2.ElasticLoadBalancingV2Client;
import software.amazon.awssdk.services.s3.S3Client;
import software.amazon.awssdk.services.ssm.SsmClient;
import software.amazon.awssdk.services.secretsmanager.SecretsManagerClient;
public final class AwsServices implements AutoCloseable {
 public final Ec2Client ec2; public final AutoScalingClient asg; public final ElasticLoadBalancingV2Client elb;
 public final S3Client s3; public final SsmClient ssm; public final SecretsManagerClient secrets;
 public AwsServices(String region){Region r=Region.of(region); ec2=Ec2Client.builder().region(r).build();
  asg=AutoScalingClient.builder().region(r).build(); elb=ElasticLoadBalancingV2Client.builder().region(r).build();
  s3=S3Client.builder().region(r).build(); ssm=SsmClient.builder().region(r).build(); secrets=SecretsManagerClient.builder().region(r).build();}
 public void close(){ec2.close();asg.close();elb.close();s3.close();ssm.close();secrets.close();}
}
