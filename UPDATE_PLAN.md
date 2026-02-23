# pGina Update Plan for Windows 11 and MySQL 8/MariaDB Compatibility

## Current Issues Identified

1. **Outdated Dependencies**:
   - .NET Framework v4.0 (released 2010)
   - log4net v1.2.10 (released 2006)
   - MySQL Connector/NET v6.5.4 (released 2012)
   - Visual Studio 2010 project files

2. **Architecture Limitations**:
   - Primarily configured for x86 architecture
   - Mixed native (C++) and managed (.NET) components
   - Complex interdependencies between projects

3. **Database Compatibility**:
   - Uses older MySQL connector incompatible with MySQL 8
   - Authentication methods changed in MySQL 8

## Required Updates

### 1. Modernize .NET Components

Update all .NET projects to target .NET Framework 4.8 or consider migration to .NET Core/.NET 6+:

```xml
<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
```

### 2. Update Dependencies

- Update log4net to latest stable version (v2.0.15+)
- Update MySQL connector to version compatible with MySQL 8 / MariaDB
- Replace MySQL.Data with MySqlConnector or Pomelo.EntityFrameworkCore.MySql for better compatibility

### 3. Project File Updates

Convert old MSBuild formats to newer schema versions and fix platform configurations:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>
</Project>
```

### 4. MySQL 8/MariaDB Specific Changes

- Update connection strings to handle caching_sha2_password authentication
- Update MySQL connector to handle new authentication methods
- Consider supporting both MySQL and MariaDB with conditional logic

### 5. Windows 11 Compatibility

- Update Credential Provider implementation to comply with Windows 11 requirements
- Ensure proper architecture targeting (x64 preferred)
- Verify COM registration works properly on Windows 11
- Update any deprecated Windows APIs

## Implementation Steps

### Phase 1: Dependency Updates

1. Update NuGet packages:
   - log4net to latest version
   - MySQL connector to version supporting MySQL 8/MariaDB
   - Update all project references

2. Update project files:
   - Convert to PackageReference format
   - Update target framework
   - Fix assembly binding redirects

### Phase 2: Database Layer Updates

1. Modify MySQLAuth plugin to work with MySQL 8:
   - Update connection string handling
   - Support new authentication methods
   - Add MariaDB compatibility

2. Update MySQLLogger plugin similarly

### Phase 3: Architecture Updates

1. Update platform targets to x64
2. Verify native code compatibility with Windows 11
3. Update installation scripts for modern Windows

### Phase 4: Testing and Validation

1. Test on Windows 11 environment
2. Verify MySQL 8/MariaDB connectivity
3. Ensure credential provider functionality
4. Validate authentication workflows

## Files to Update

- All `.csproj` files for dependency and framework updates
- `MySqlUserDataSource.cs` for MySQL 8 compatibility
- Connection string handling in settings
- Installation scripts and manifests
- Native C++ components if needed

## Risks and Considerations

- Breaking changes in MySQL 8 authentication may require database schema changes
- Windows 11 may have stricter requirements for credential providers
- Older .NET Framework versions may not support newer MySQL features
- Native code may need recompilation for Windows 11 compatibility