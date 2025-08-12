<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" name="OblakProject" generation="1" functional="0" release="0" Id="f556c06a-6402-4cbc-b606-fbc6c9ad9f99" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="OblakProjectGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="MovieService_WebRole1:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/OblakProject/OblakProjectGroup/LB:MovieService_WebRole1:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="MovieService_WebRole1:DataConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/OblakProject/OblakProjectGroup/MapMovieService_WebRole1:DataConnectionString" />
          </maps>
        </aCS>
        <aCS name="MovieService_WebRole1:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/OblakProject/OblakProjectGroup/MapMovieService_WebRole1:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="MovieService_WebRole1Instances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/OblakProject/OblakProjectGroup/MapMovieService_WebRole1Instances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:MovieService_WebRole1:Endpoint1">
          <toPorts>
            <inPortMoniker name="/OblakProject/OblakProjectGroup/MovieService_WebRole1/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapMovieService_WebRole1:DataConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/OblakProject/OblakProjectGroup/MovieService_WebRole1/DataConnectionString" />
          </setting>
        </map>
        <map name="MapMovieService_WebRole1:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/OblakProject/OblakProjectGroup/MovieService_WebRole1/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapMovieService_WebRole1Instances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/OblakProject/OblakProjectGroup/MovieService_WebRole1Instances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="MovieService_WebRole1" generation="1" functional="0" release="0" software="C:\Users\Filip\Desktop\Oblak Projekat\OblakProject\csx\Debug\roles\MovieService_WebRole1" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="-1" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="DataConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;MovieService_WebRole1&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;MovieService_WebRole1&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/OblakProject/OblakProjectGroup/MovieService_WebRole1Instances" />
            <sCSPolicyUpdateDomainMoniker name="/OblakProject/OblakProjectGroup/MovieService_WebRole1UpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/OblakProject/OblakProjectGroup/MovieService_WebRole1FaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="MovieService_WebRole1UpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="MovieService_WebRole1FaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="MovieService_WebRole1Instances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="b5fd5224-72d9-4d8a-8d00-865b92867425" ref="Microsoft.RedDog.Contract\ServiceContract\OblakProjectContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="5c92ed45-9f04-4c8d-a164-a4f6d1a82d36" ref="Microsoft.RedDog.Contract\Interface\MovieService_WebRole1:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/OblakProject/OblakProjectGroup/MovieService_WebRole1:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>