package com.stackpulse.aws.service;

import software.amazon.awssdk.services.autoscaling.model.CreateAutoScalingGroupRequest;
import software.amazon.awssdk.services.autoscaling.model.DescribeAutoScalingGroupsRequest;
import software.amazon.awssdk.services.autoscaling.model.LaunchTemplateSpecification;
import software.amazon.awssdk.services.autoscaling.model.PredefinedMetricSpecification;
import software.amazon.awssdk.services.autoscaling.model.PutScalingPolicyRequest;
import software.amazon.awssdk.services.autoscaling.model.PutScheduledUpdateGroupActionRequest;
import software.amazon.awssdk.services.autoscaling.model.TargetTrackingConfiguration;
import software.amazon.awssdk.services.autoscaling.model.UpdateAutoScalingGroupRequest;
import software.amazon.awssdk.services.autoscaling.model.MetricType;

import software.amazon.awssdk.services.ec2.model.LaunchTemplateIamInstanceProfileSpecificationRequest;
import software.amazon.awssdk.services.ec2.model.LaunchTemplateBlockDeviceMappingRequest;
import software.amazon.awssdk.services.ec2.model.LaunchTemplateEbsBlockDeviceRequest;
import software.amazon.awssdk.services.ec2.model.RequestLaunchTemplateData;

import java.util.Base64;

public final class AutoScalingService {
    private final AwsServices aws;

    public AutoScalingService(AwsServices aws) {
        this.aws = aws;
    }

    public String launchTemplate(
            String name,
            String amiId,
            String instanceType,
            String securityGroupId,
            String instanceProfileName,
            String rootVolumeSizeGiB,
            String userData) {

        RequestLaunchTemplateData launchTemplateData =
                RequestLaunchTemplateData.builder()
                        .imageId(amiId)
                        .instanceType(instanceType)
                        .securityGroupIds(securityGroupId)
                        .iamInstanceProfile(
                                LaunchTemplateIamInstanceProfileSpecificationRequest.builder()
                                        .name(instanceProfileName)
                                        .build())
                        .userData(Base64.getEncoder().encodeToString(userData.getBytes()))
                        .blockDeviceMappings(
                                LaunchTemplateBlockDeviceMappingRequest.builder()
                                        .deviceName("/dev/xvda")
                                        .ebs(
                                                LaunchTemplateEbsBlockDeviceRequest.builder()
                                                        .volumeSize(Integer.parseInt(rootVolumeSizeGiB))
                                                        .volumeType("gp3")
                                                        .deleteOnTermination(true)
                                                        .build())
                                        .build())
                        .build();

        var existing = aws.ec2.describeLaunchTemplates(
                software.amazon.awssdk.services.ec2.model.DescribeLaunchTemplatesRequest.builder()
                        .launchTemplateNames(name)
                        .build());

        if (existing.launchTemplates().isEmpty()) {
            return aws.ec2.createLaunchTemplate(
                    software.amazon.awssdk.services.ec2.model.CreateLaunchTemplateRequest.builder()
                            .launchTemplateName(name)
                            .launchTemplateData(launchTemplateData)
                            .versionDescription("StackPulse deployment")
                            .build())
                    .launchTemplate()
                    .launchTemplateId();
        }

        String launchTemplateId = existing.launchTemplates().getFirst().launchTemplateId();

        aws.ec2.createLaunchTemplateVersion(
                software.amazon.awssdk.services.ec2.model.CreateLaunchTemplateVersionRequest.builder()
                        .launchTemplateId(launchTemplateId)
                        .sourceVersion("$Latest")
                        .launchTemplateData(launchTemplateData)
                        .versionDescription("StackPulse deployment update")
                        .build());

        return launchTemplateId;
    }

    public void asg(
            String name,
            String launchTemplateId,
            String subnetIds,
            String targetGroupArn,
            int min,
            int desired,
            int max) {

        var found = aws.asg.describeAutoScalingGroups(
                DescribeAutoScalingGroupsRequest.builder()
                        .autoScalingGroupNames(name)
                        .build());

        var launchTemplateSpecification =
                LaunchTemplateSpecification.builder()
                        .launchTemplateId(launchTemplateId)
                        .version("$Latest")
                        .build();

        if (found.autoScalingGroups().isEmpty()) {
            aws.asg.createAutoScalingGroup(
                    CreateAutoScalingGroupRequest.builder()
                            .autoScalingGroupName(name)
                            .launchTemplate(launchTemplateSpecification)
                            .vpcZoneIdentifier(subnetIds)
                            .minSize(min)
                            .desiredCapacity(desired)
                            .maxSize(max)
                            .targetGroupARNs(targetGroupArn)
                            .healthCheckType("ELB")
                            .healthCheckGracePeriod(120)
                            .build());
        } else {
            aws.asg.updateAutoScalingGroup(
                    UpdateAutoScalingGroupRequest.builder()
                            .autoScalingGroupName(name)
                            .launchTemplate(launchTemplateSpecification)
                            .vpcZoneIdentifier(subnetIds)
                            .minSize(min)
                            .desiredCapacity(desired)
                            .maxSize(max)
                            .build());
        }
    }

    public void cpu(String autoScalingGroupName, double targetPercent) {
        aws.asg.putScalingPolicy(
                PutScalingPolicyRequest.builder()
                        .autoScalingGroupName(autoScalingGroupName)
                        .policyName(autoScalingGroupName + "-cpu")
                        .policyType("TargetTrackingScaling")
                        .targetTrackingConfiguration(
                                TargetTrackingConfiguration.builder()
                                        .predefinedMetricSpecification(
                                                PredefinedMetricSpecification.builder()
                                                        .predefinedMetricType(
                                                                MetricType.ASG_AVERAGE_CPU_UTILIZATION)
                                                        .build())
                                        .targetValue(targetPercent)
                                        .build())
                        .build());
    }

    public void schedule(
            String autoScalingGroupName,
            String actionName,
            int desired,
            String recurrence) {

        aws.asg.putScheduledUpdateGroupAction(
                PutScheduledUpdateGroupActionRequest.builder()
                        .autoScalingGroupName(autoScalingGroupName)
                        .scheduledActionName(actionName)
                        .desiredCapacity(desired)
                        .minSize(desired)
                        .maxSize(Math.max(2, desired))
                        .recurrence(recurrence)
                        .build());
    }
}
