package com.stackpulse.aws.cli;
import com.stackpulse.aws.config.*;
import com.stackpulse.aws.service.*;
import java.nio.file.Path;
public final class CommandHandler {
 private CommandHandler(){}
 public static void run(String[] args)throws Exception{
  if(args.length<2){usage();return;} String cmd=args[0], name=args[1]; Path p=Path.of("config/production.json");
  for(int i=2;i<args.length-1;i++)if("--config".equals(args[i]))p=Path.of(args[i+1]);
  var c=ConfigLoader.load(p); var app=ConfigLoader.app(c,name);
  try(var aws=new AwsServices(c.region())){
   if(cmd.equals("deploy")) deploy(c,app,aws);
   else if(cmd.equals("package")) System.out.println("Run the Jenkins/builder pipeline to create the immutable AMI for "+name+"; then set amiId in config.");
   else if(cmd.equals("package-deploy")) {System.out.println("Package using Jenkins/builder, then deploy after amiId is available."); deploy(c,app,aws);}
   else usage();
  }
 }
 static void deploy(Models.DeploymentConfig c,Models.ApplicationConfig app,AwsServices aws){
  if(app.amiId()==null||app.amiId().isBlank())throw new IllegalArgumentException("amiId is required for "+app.name());
  var alb=new AlbService(aws); String tg=alb.targetGroup("sp-"+app.name()+"-tg",c.vpcId(),app.containerPort());
  var as=new AutoScalingService(aws); String asg="sp-"+app.name()+"-asg";
  String lt=as.launchTemplate("sp-"+app.name()+"-lt",app.amiId(),c.applicationInstanceType(),
   c.applicationSecurityGroupId(),c.instanceProfileName(),c.rootVolumeSizeGiB(),
   "#!/bin/bash\n/opt/stackpulse/instance-bootstrap.sh "+app.name()+"\n");
  as.asg(asg,lt,c.privateSubnetIds(),tg,app.minInstances(),app.desiredInstances(),app.maxInstances());
  as.cpu(asg,app.cpuTargetPercent());
  if(c.httpsListenerArn()!=null&&!c.httpsListenerArn().isBlank())
   alb.hostRule(c.httpsListenerArn(),app.domain(),tg,app.name().equalsIgnoreCase("stackpulse")?100:110);
  if(c.startCron()!=null)as.schedule(asg,asg+"-start",1,c.startCron());
  if(c.stopCron()!=null)as.schedule(asg,asg+"-stop",0,c.stopCron());
  System.out.println("Deployment configured: "+asg);
 }
 static void usage(){System.out.println("Usage: java -jar stackpulse-aws-deployment.jar <deploy|package|package-deploy> <app> --config config/production.json");}
}
