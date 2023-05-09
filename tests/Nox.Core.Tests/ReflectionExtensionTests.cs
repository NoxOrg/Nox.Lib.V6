using System;
using System.Reflection;
using System.Reflection.Emit;
using Nox.Core.Extensions;
using NUnit.Framework;

namespace Nox.Core.Tests;

public class ReflectionExtensionTests
{
    [Test]
    public void Can_Dynamically_Add_GetSet_Property_to_an_object()
    {
        var asmName = new AssemblyName("TestPoco");
        var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
        var moduleBuilder = asmBuilder.DefineDynamicModule(asmName.Name!);
        var typeBuilder = moduleBuilder.DefineType("Person", TypeAttributes.Public, null);
        var fb = typeBuilder.AddPublicGetSetProperty("Lastname", typeof(string));
        typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        var type = typeBuilder.CreateType();
        var instance = Activator.CreateInstance(type!);
        var prop = instance!.GetType().GetProperties()[0]; 
        Assert.That(prop.Name, Is.EqualTo("Lastname"));
        Assert.That(prop.PropertyType.Name, Is.EqualTo("String"));
    }
    
    [Test]
    public void Can_Dynamically_Add_GetSet_List_Property_to_an_object()
    {
        var asmName = new AssemblyName("TestPoco");
        var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
        var moduleBuilder = asmBuilder.DefineDynamicModule(asmName.Name!);
        var typeBuilder = moduleBuilder.DefineType("Person", TypeAttributes.Public, null);
        var fb = typeBuilder.AddPublicGetSetPropertyAsList("Names", typeof(string));
        typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        var type = typeBuilder.CreateType();
        var instance = Activator.CreateInstance(type!);
        var prop = instance!.GetType().GetProperties()[0]; 
        Assert.That(prop.Name, Is.EqualTo("Names"));
        Assert.That(prop.PropertyType.Name, Is.EqualTo("Collection`1"));
    }
}