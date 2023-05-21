using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace Nox.Core.Extensions
{
    public static class ReflectionExtensions
    {
        public static PropertyBuilder AddPublicGetSetPropertyAsList(this TypeBuilder tb, string propertyName, Type listType)
        {
            return AddPublicGetSetProperty(tb, propertyName, typeof(Collection<>).MakeGenericType(new[] { listType }));
        }

        public static PropertyBuilder AddPublicGetSetProperty(this TypeBuilder tb, 
            string propertyName, Type propertyType)
        {
            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            var fb = tb.DefineField("_" + propertyName.ToLower(), propertyType, FieldAttributes.Private);

            PropertyBuilder pb = tb.DefineProperty(propertyName,
                PropertyAttributes.HasDefault,
                propertyType,
                null);

            MethodBuilder mbGetAccessor = tb.DefineMethod(
                "get_" + propertyName,
                getSetAttr,
                propertyType,
                Type.EmptyTypes);

            ILGenerator getterIL = mbGetAccessor.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fb);
            getterIL.Emit(OpCodes.Ret);

            MethodBuilder mbSetAccessor = tb.DefineMethod(
                "set_" + propertyName,
                getSetAttr,
                null,
                new Type[] { propertyType });

            ILGenerator idSetIL = mbSetAccessor.GetILGenerator();
            idSetIL.Emit(OpCodes.Ldarg_0);
            idSetIL.Emit(OpCodes.Ldarg_1);
            idSetIL.Emit(OpCodes.Stfld, fb);
            idSetIL.Emit(OpCodes.Ret);

            pb.SetGetMethod(mbGetAccessor);
            pb.SetSetMethod(mbSetAccessor);

            return pb;
        }
    }
}
