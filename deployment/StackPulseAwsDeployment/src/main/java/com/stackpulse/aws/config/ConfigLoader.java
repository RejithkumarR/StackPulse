package com.stackpulse.aws.config;
import com.fasterxml.jackson.databind.ObjectMapper;
import java.nio.file.*;
public final class ConfigLoader {
 private ConfigLoader(){}
 public static Models.DeploymentConfig load(Path p)throws Exception{
  return new ObjectMapper().readValue(Files.readString(p),Models.DeploymentConfig.class);
 }
 public static Models.ApplicationConfig app(Models.DeploymentConfig c,String n){
  return c.applications().stream().filter(a->a.name().equalsIgnoreCase(n)).findFirst()
   .orElseThrow(()->new IllegalArgumentException("Application not found: "+n));
 }
}
