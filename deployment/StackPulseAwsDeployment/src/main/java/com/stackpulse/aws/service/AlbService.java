package com.stackpulse.aws.service;
import software.amazon.awssdk.services.elasticloadbalancingv2.model.*;
public final class AlbService {
 private final AwsServices a; public AlbService(AwsServices a){this.a=a;}
 public String targetGroup(String name,String vpc,int port){
  try {var x=a.elb.describeTargetGroups(DescribeTargetGroupsRequest.builder().names(name).build());
   if(!x.targetGroups().isEmpty()) return x.targetGroups().getFirst().targetGroupArn();} catch(Exception ignored){}
  return a.elb.createTargetGroup(CreateTargetGroupRequest.builder().name(name).protocol(ProtocolEnum.HTTP)
   .port(port).vpcId(vpc).targetType(TargetTypeEnum.INSTANCE).healthCheckPath("/health").build())
   .targetGroups().getFirst().targetGroupArn();
 }
 public void hostRule(String listener,String host,String tg,int priority){
  try{a.elb.createRule(CreateRuleRequest.builder().listenerArn(listener).priority(priority)
   .conditions(RuleCondition.builder().field("host-header").hostHeaderConfig(HostHeaderConditionConfig.builder().values(host).build()).build())
   .actions(Action.builder().type(ActionTypeEnum.FORWARD).forwardConfig(ForwardActionConfig.builder()
   .targetGroups(TargetGroupTuple.builder().targetGroupArn(tg).weight(1).build()).build()).build()).build());}
  catch(Exception e){System.out.println("ALB rule may already exist: "+e.getMessage());}
 }
}
