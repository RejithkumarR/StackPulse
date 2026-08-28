package com.stackpulse.aws.config;
import java.util.List;
public final class Models {
 private Models(){}
 public record ApplicationConfig(String name,String domain,String amiId,String amiName,String sourcePath,
  String dockerfile,String composeFile,String imageName,int containerPort,int minInstances,int desiredInstances,
  int maxInstances,double cpuTargetPercent,String secretPrefix,String parameterPrefix){}
 public record DeploymentConfig(String region,String vpcId,String privateSubnetIds,String applicationSecurityGroupId,
  String instanceProfileName,String applicationInstanceType,String rootVolumeSizeGiB,String httpsListenerArn,
  String artifactBucket,String startCron,String stopCron,List<ApplicationConfig> applications){}
}
