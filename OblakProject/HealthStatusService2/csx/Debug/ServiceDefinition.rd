<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" name="HealthStatusService2" generation="1" functional="0" release="0" Id="7a8497bf-8f95-42a1-b836-af019dd9eb36" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="HealthStatusService2Group" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="HealthStatusService.WebRole:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/HealthStatusService2/HealthStatusService2Group/LB:HealthStatusService.WebRole:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="HealthStatusService.WebRole:DataConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/HealthStatusService2/HealthStatusService2Group/MapHealthStatusService.WebRole:DataConnectionString" />
          </maps>
        </aCS>
        <aCS name="HealthStatusService.WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/HealthStatusService2/HealthStatusService2Group/MapHealthStatusService.WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="HealthStatusService.WebRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/HealthStatusService2/HealthStatusService2Group/MapHealthStatusService.WebRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:HealthStatusService.WebRole:Endpoint1">
          <toPorts>
            <inPortMoniker name="/HealthStatusService2/HealthStatusService2Group/HealthStatusService.WebRole/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapHealthStatusService.WebRole:DataConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/HealthStatusService2/HealthStatusService2Group/HealthStatusService.WebRole/DataConnectionString" />
          </setting>
        </map>
        <map name="MapHealthStatusService.WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/HealthStatusService2/HealthStatusService2Group/HealthStatusService.WebRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapHealthStatusService.WebRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/HealthStatusService2/HealthStatusService2Group/HealthStatusService.WebRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="HealthStatusService.WebRole" generation="1" functional="0" release="0" software="C:\Users\Duska\Desktop\rca2\OblakProjekat\HealthStatusService2\csx\Debug\roles\HealthStatusService.WebRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="-1" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="DataConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;HealthStatusService.WebRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;HealthStatusService.WebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/HealthStatusService2/HealthStatusService2Group/HealthStatusService.WebRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/HealthStatusService2/HealthStatusService2Group/HealthStatusService.WebRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/HealthStatusService2/HealthStatusService2Group/HealthStatusService.WebRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="HealthStatusService.WebRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="HealthStatusService.WebRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="HealthStatusService.WebRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="9a8fe642-41c0-4bc3-a8c3-85d9c2ab80d4" ref="Microsoft.RedDog.Contract\ServiceContract\HealthStatusService2Contract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="5e7e982e-7751-4bd9-97a5-f1b8f4aac5f9" ref="Microsoft.RedDog.Contract\Interface\HealthStatusService.WebRole:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/HealthStatusService2/HealthStatusService2Group/HealthStatusService.WebRole:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>