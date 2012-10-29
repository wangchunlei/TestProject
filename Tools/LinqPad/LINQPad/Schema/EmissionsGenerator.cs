namespace LINQPad.Schema
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext.DbSchema;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;

    internal class EmissionsGenerator
    {
        private ConstructorInfo _actionTConstructorOpen = typeof(Action<>).GetConstructors().FirstOrDefault<ConstructorInfo>();
        private Dictionary<Relationship, PropertyBuilder> _entityRefProps = new Dictionary<Relationship, PropertyBuilder>();
        private Dictionary<Relationship, MethodBuilder> _entityRefPropSetMethods = new Dictionary<Relationship, MethodBuilder>();
        private static ConstructorInfo _esConstructorOpen;
        private Dictionary<string, TypeBuilder> _tableTypes = new Dictionary<string, TypeBuilder>();
        public readonly Database Schema;

        static EmissionsGenerator()
        {
            _esConstructorOpen = (from <>h__TransparentIdentifier1f in (from c in typeof(EntitySet<>).GetConstructors() select new { c = c, args = c.GetParameters() }).Where(delegate (<>f__AnonymousType40<ConstructorInfo, ParameterInfo[]> <>h__TransparentIdentifier1f) {
                if ((<>h__TransparentIdentifier1f.args.Length == 2) && <>h__TransparentIdentifier1f.args.All<ParameterInfo>(a => a.ParameterType.IsGenericType))
                {
                }
                return (CS$<>9__CachedAnonymousMethodDelegate29 == null) && <>h__TransparentIdentifier1f.args.All<ParameterInfo>(CS$<>9__CachedAnonymousMethodDelegate29);
            }) select <>h__TransparentIdentifier1f.c).FirstOrDefault<ConstructorInfo>();
        }

        private EmissionsGenerator(Database schema)
        {
            this.Schema = schema;
        }

        private static string BracketsForSql(string sqlName)
        {
            if (string.IsNullOrEmpty(sqlName))
            {
                return sqlName;
            }
            return ("[" + sqlName + "]");
        }

        private static void DefineForwardingConstructor(TypeBuilder definingType, Type baseType, Type[] parameters, Type[] baseParams, string[] paramNames, Action<ILGenerator> paramIL)
        {
            ConstructorBuilder builder = definingType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameters);
            int num = 0;
            foreach (string str in paramNames)
            {
                builder.DefineParameter(++num, ParameterAttributes.None, str);
            }
            ILGenerator iLGenerator = builder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            paramIL(iLGenerator);
            ConstructorInfo constructor = baseType.GetConstructor(baseParams);
            iLGenerator.Emit(OpCodes.Call, constructor);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void EmitCanonFunctionMethod(TypeBuilder dcType, string methName, MethodInfo parentMethod, Action<ILGenerator> dcFieldLoader, SchemaObject funcInfo)
        {
            if (parentMethod != null)
            {
                FunctionGenerator generator = new FunctionGenerator(this, dcType, funcInfo, methName, MethodAttributes.Public);
                if (generator.Method != null)
                {
                    ILGenerator iLGenerator = generator.Method.GetILGenerator();
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    dcFieldLoader(iLGenerator);
                    int position = 0;
                    foreach (Parameter parameter in funcInfo.Parameters)
                    {
                        position++;
                        generator.Method.DefineParameter(position, ParameterAttributes.None, parameter.ClrName);
                        iLGenerator.Emit(OpCodes.Ldarg, position);
                    }
                    iLGenerator.Emit(OpCodes.Call, parentMethod);
                    iLGenerator.Emit(OpCodes.Ret);
                }
            }
        }

        private void EmitChildRelation(ILGenerator constructorGen, TypeBuilder t, Relationship r)
        {
            string parentFieldName = r.ParentFieldName;
            TypeBuilder builder = this._tableTypes[r.ChildTable.DotNetName];
            Type type = typeof(EntitySet<>).MakeGenericType(new Type[] { builder });
            Type type2 = typeof(Action<>).MakeGenericType(new Type[] { builder });
            FieldBuilder field = t.DefineField(parentFieldName, type, FieldAttributes.Private);
            MethodBuilder meth = t.DefineMethod("attach" + parentFieldName, MethodAttributes.Private, typeof(void), new TypeBuilder[] { builder });
            ILGenerator iLGenerator = meth.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Callvirt, this._entityRefPropSetMethods[r]);
            iLGenerator.Emit(OpCodes.Ret);
            MethodBuilder builder4 = t.DefineMethod("detach" + parentFieldName, MethodAttributes.Private, typeof(void), new TypeBuilder[] { builder });
            ILGenerator generator2 = builder4.GetILGenerator();
            generator2.Emit(OpCodes.Ldarg_1);
            generator2.Emit(OpCodes.Ldnull);
            generator2.Emit(OpCodes.Callvirt, this._entityRefPropSetMethods[r]);
            generator2.Emit(OpCodes.Ret);
            ConstructorInfo constructor = TypeBuilder.GetConstructor(type2, this._actionTConstructorOpen);
            ConstructorInfo con = TypeBuilder.GetConstructor(type, _esConstructorOpen);
            constructorGen.Emit(OpCodes.Ldarg_0);
            constructorGen.Emit(OpCodes.Ldarg_0);
            constructorGen.Emit(OpCodes.Ldftn, meth);
            constructorGen.Emit(OpCodes.Newobj, constructor);
            constructorGen.Emit(OpCodes.Ldarg_0);
            constructorGen.Emit(OpCodes.Ldftn, builder4);
            constructorGen.Emit(OpCodes.Newobj, constructor);
            constructorGen.Emit(OpCodes.Newobj, con);
            constructorGen.Emit(OpCodes.Stfld, field);
            PropertyBuilder builder5 = t.DefineProperty(r.PropNameForParent, PropertyAttributes.HasDefault, type, null);
            MethodBuilder mdBuilder = t.DefineMethod("get_" + r.PropNameForParent, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, type, new Type[0]);
            ILGenerator generator3 = mdBuilder.GetILGenerator();
            generator3.Emit(OpCodes.Ldarg_0);
            generator3.Emit(OpCodes.Ldfld, field);
            generator3.Emit(OpCodes.Ret);
            MethodBuilder builder7 = t.DefineMethod("set_" + r.PropNameForParent, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, typeof(void), new Type[] { type });
            ILGenerator generator4 = builder7.GetILGenerator();
            generator4.Emit(OpCodes.Ldarg_0);
            generator4.Emit(OpCodes.Ldfld, field);
            generator4.Emit(OpCodes.Ldarg_1);
            MethodInfo method = typeof(EntitySet<>).GetMethod("Assign");
            MethodInfo methodInfo = TypeBuilder.GetMethod(type, method);
            generator4.EmitCall(OpCodes.Callvirt, methodInfo, null);
            generator4.Emit(OpCodes.Ret);
            builder5.SetGetMethod(mdBuilder);
            builder5.SetSetMethod(builder7);
            object[] propertyValues = new object[] { r.Name, parentFieldName, string.Join(",", (from c in r.ChildCols select c.PropertyName).ToArray<string>()), string.Join(",", (from c in r.ParentCols select c.PropertyName).ToArray<string>()) };
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(typeof(AssociationAttribute).GetConstructor(new Type[0]), new object[0], new PropertyInfo[] { typeof(AssociationAttribute).GetProperty("Name"), typeof(AssociationAttribute).GetProperty("Storage"), typeof(AssociationAttribute).GetProperty("OtherKey"), typeof(AssociationAttribute).GetProperty("ThisKey") }, propertyValues);
            builder5.SetCustomAttribute(customBuilder);
        }

        private void EmitColumn(TypeBuilder t, SqlColumn c)
        {
            string cSharpTypeName = c.CSharpTypeName;
            FieldBuilder builder = t.DefineField(c.PropertyName, c.ClrType, FieldAttributes.Public);
            ConstructorInfo constructor = typeof(ColumnAttribute).GetConstructor(new Type[0]);
            List<PropertyInfo> list = new List<PropertyInfo>();
            List<object> list2 = new List<object>();
            if (c.PropertyName != c.ColumnName)
            {
                list.Add(typeof(ColumnAttribute).GetProperty("Name"));
                string columnName = c.ColumnName;
                if (columnName.Contains<char>('.'))
                {
                    columnName = "[" + columnName + "]";
                }
                list2.Add(columnName);
            }
            if (c.IsKey)
            {
                list.Add(typeof(ColumnAttribute).GetProperty("IsPrimaryKey"));
                list2.Add(true);
            }
            if ((c.IsAutoGen || c.IsComputed) || c.IsTimeStamp)
            {
                list.Add(typeof(ColumnAttribute).GetProperty("IsDbGenerated"));
                list2.Add(true);
            }
            if (c.IsTimeStamp)
            {
                list.Add(typeof(ColumnAttribute).GetProperty("IsVersion"));
                list2.Add(true);
            }
            if (!(c.ClrType.IsValueType || c.IsNullable))
            {
                list.Add(typeof(ColumnAttribute).GetProperty("CanBeNull"));
                list2.Add(c.IsNullable);
            }
            string fullSqlTypeDeclaration = c.GetFullSqlTypeDeclaration();
            if (!new string[] { "hierarchyid", "geography", "geometry" }.Contains<string>(c.SqlType.Name.ToLowerInvariant()))
            {
                list.Add(typeof(ColumnAttribute).GetProperty("DbType"));
                list2.Add(fullSqlTypeDeclaration);
            }
            list.Add(typeof(ColumnAttribute).GetProperty("UpdateCheck"));
            list2.Add(UpdateCheck.Never);
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(constructor, new object[0], list.ToArray(), list2.ToArray());
            builder.SetCustomAttribute(customBuilder);
            if (c.IsKey)
            {
                builder.SetCustomAttribute(new CustomAttributeBuilder(typeof(LINQPadPKeyAttribute).GetConstructor(new Type[0]), new object[0]));
            }
        }

        private static void EmitDataContextConstructors(TypeBuilder type, string cxString)
        {
            FieldBuilder mappingField = type.DefineField("_mapping", typeof(MappingSource), FieldAttributes.Static | FieldAttributes.Private);
            ILGenerator iLGenerator = type.DefineConstructor(MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, new Type[0]).GetILGenerator();
            iLGenerator.Emit(OpCodes.Newobj, typeof(AttributeMappingSource).GetConstructor(new Type[0]));
            iLGenerator.Emit(OpCodes.Stsfld, mappingField);
            iLGenerator.Emit(OpCodes.Ret);
            DefineForwardingConstructor(type, typeof(DataContextBase), new Type[0], new Type[] { typeof(string), typeof(MappingSource) }, new string[0], delegate (ILGenerator gen) {
                gen.Emit(OpCodes.Ldstr, cxString);
                gen.Emit(OpCodes.Ldsfld, mappingField);
            });
            DefineForwardingConstructor(type, typeof(DataContextBase), new Type[] { typeof(string) }, new Type[] { typeof(string), typeof(MappingSource) }, new string[] { "connectionString" }, delegate (ILGenerator gen) {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldsfld, mappingField);
            });
            DefineForwardingConstructor(type, typeof(DataContextBase), new Type[] { typeof(IDbConnection) }, new Type[] { typeof(IDbConnection), typeof(MappingSource) }, new string[] { "connection" }, delegate (ILGenerator gen) {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldsfld, mappingField);
            });
        }

        private void EmitDataContextMembers(TypeBuilder dcType, Action<ILGenerator> dcFieldLoader, TypeBuilder parentDC, string clrTypeNamePrefix, string objectPrefix)
        {
            foreach (SchemaObject obj2 in this.Schema.Objects.Values)
            {
                if ((obj2 is Table) || (obj2 is View))
                {
                    this.EmitTableProperty(dcType, dcFieldLoader, obj2);
                }
                else if ((obj2 is TableFunction) || (obj2 is ScalarFunction))
                {
                    if (parentDC == null)
                    {
                        this.EmitFunctionMethod(dcType, obj2.PropertyName, obj2, objectPrefix, MethodAttributes.Public);
                    }
                    else
                    {
                        MethodInfo parentMethod = this.EmitFunctionMethod(parentDC, "_" + clrTypeNamePrefix + "_" + obj2.PropertyName, obj2, objectPrefix, MethodAttributes.Assembly);
                        this.EmitCanonFunctionMethod(dcType, obj2.PropertyName, parentMethod, dcFieldLoader, obj2);
                    }
                }
                else if (obj2 is StoredProc)
                {
                    this.EmitStoredProc(dcType, dcFieldLoader, obj2, objectPrefix, false);
                    if (obj2.Parameters.Count > 0)
                    {
                        this.EmitStoredProc(dcType, dcFieldLoader, obj2, objectPrefix, true);
                    }
                }
            }
        }

        private MethodInfo EmitFunctionMethod(TypeBuilder dcType, string methName, SchemaObject funcInfo, string objectPrefix, MethodAttributes accessibility)
        {
            FunctionGenerator generator = new FunctionGenerator(this, dcType, funcInfo, methName, accessibility);
            if (generator.Method == null)
            {
                return null;
            }
            generator.AddAttributes(objectPrefix);
            ILGenerator iLGenerator = generator.Method.GetILGenerator();
            LocalBuilder local = iLGenerator.DeclareLocal(typeof(object[]));
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null));
            iLGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
            iLGenerator.Emit(OpCodes.Ldc_I4, funcInfo.Parameters.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(object));
            iLGenerator.Emit(OpCodes.Stloc, local);
            int arg = 0;
            foreach (Parameter parameter in funcInfo.Parameters)
            {
                iLGenerator.Emit(OpCodes.Ldloc, local);
                iLGenerator.Emit(OpCodes.Ldc_I4, arg);
                iLGenerator.Emit(OpCodes.Ldarg, (int) (arg + 1));
                if (parameter.ClrType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Box, parameter.ClrType);
                }
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                arg++;
            }
            iLGenerator.Emit(OpCodes.Ldloc, local);
            MethodInfo meth = typeof(DataContext).GetMethod((funcInfo is TableFunction) ? "CreateMethodCallQuery" : "ExecuteMethodCall", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object), typeof(MethodInfo), typeof(object[]) }, null);
            if (funcInfo is TableFunction)
            {
                meth = meth.MakeGenericMethod(new Type[] { generator.ResultType });
            }
            iLGenerator.Emit(OpCodes.Call, meth);
            if (funcInfo is ScalarFunction)
            {
                iLGenerator.Emit(OpCodes.Callvirt, typeof(IExecuteResult).GetProperty("ReturnValue").GetGetMethod());
                iLGenerator.Emit(generator.ResultType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, generator.ResultType);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return generator.Method;
        }

        private void EmitOneToOneOnChildSide(ILGenerator constructorGen, TypeBuilder t, Relationship r)
        {
            string parentFieldName = r.ParentFieldName;
            TypeBuilder returnType = this._tableTypes[r.ChildTable.DotNetName];
            Type type = typeof(EntityRef<>).MakeGenericType(new Type[] { returnType });
            FieldBuilder field = t.DefineField(parentFieldName, type, FieldAttributes.Private);
            PropertyBuilder builder3 = t.DefineProperty(r.PropNameForParent, PropertyAttributes.HasDefault, returnType, null);
            MethodBuilder mdBuilder = t.DefineMethod("get_" + r.PropNameForParent, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, returnType, new Type[0]);
            ILGenerator iLGenerator = mdBuilder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldflda, field);
            MethodInfo getMethod = typeof(EntityRef<>).GetProperty("Entity").GetGetMethod();
            MethodInfo method = TypeBuilder.GetMethod(type, getMethod);
            iLGenerator.EmitCall(OpCodes.Call, method, null);
            iLGenerator.Emit(OpCodes.Ret);
            MethodBuilder builder5 = t.DefineMethod("set_" + r.PropNameForParent, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, typeof(void), new Type[] { returnType });
            ILGenerator generator2 = builder5.GetILGenerator();
            generator2.Emit(OpCodes.Ldarg_0);
            generator2.Emit(OpCodes.Ldflda, field);
            generator2.Emit(OpCodes.Ldarg_1);
            MethodInfo setMethod = typeof(EntityRef<>).GetProperty("Entity").GetSetMethod();
            MethodInfo methodInfo = TypeBuilder.GetMethod(type, setMethod);
            generator2.EmitCall(OpCodes.Call, methodInfo, null);
            generator2.Emit(OpCodes.Ret);
            builder3.SetGetMethod(mdBuilder);
            builder3.SetSetMethod(builder5);
            object[] propertyValues = new object[] { r.Name, parentFieldName, string.Join(",", (from c in r.ParentCols select c.PropertyName).ToArray<string>()), string.Join(",", (from c in r.ChildCols select c.PropertyName).ToArray<string>()), false, true };
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(typeof(AssociationAttribute).GetConstructor(new Type[0]), new object[0], new PropertyInfo[] { typeof(AssociationAttribute).GetProperty("Name"), typeof(AssociationAttribute).GetProperty("Storage"), typeof(AssociationAttribute).GetProperty("ThisKey"), typeof(AssociationAttribute).GetProperty("OtherKey"), typeof(AssociationAttribute).GetProperty("IsForeignKey"), typeof(AssociationAttribute).GetProperty("IsUnique") }, propertyValues);
            builder3.SetCustomAttribute(customBuilder);
        }

        private void EmitParentRelation(ILGenerator constructorGen, TypeBuilder t, Relationship r)
        {
            string childFieldName = r.ChildFieldName;
            TypeBuilder returnType = this._tableTypes[r.ParentTable.DotNetName];
            Type type = typeof(EntityRef<>).MakeGenericType(new Type[] { returnType });
            FieldBuilder field = t.DefineField(childFieldName, type, FieldAttributes.Private);
            PropertyBuilder builder3 = this._entityRefProps[r];
            MethodBuilder mdBuilder = t.DefineMethod("get_" + r.PropNameForChild, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, returnType, new Type[0]);
            ILGenerator iLGenerator = mdBuilder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldflda, field);
            MethodInfo getMethod = typeof(EntityRef<>).GetProperty("Entity").GetGetMethod();
            MethodInfo method = TypeBuilder.GetMethod(type, getMethod);
            iLGenerator.EmitCall(OpCodes.Call, method, null);
            iLGenerator.Emit(OpCodes.Ret);
            MethodBuilder builder5 = this._entityRefPropSetMethods[r];
            ILGenerator generator2 = builder5.GetILGenerator();
            generator2.Emit(OpCodes.Ldarg_0);
            generator2.Emit(OpCodes.Ldflda, field);
            generator2.Emit(OpCodes.Ldarg_1);
            MethodInfo setMethod = typeof(EntityRef<>).GetProperty("Entity").GetSetMethod();
            MethodInfo methodInfo = TypeBuilder.GetMethod(type, setMethod);
            generator2.EmitCall(OpCodes.Call, methodInfo, null);
            generator2.Emit(OpCodes.Ret);
            builder3.SetGetMethod(mdBuilder);
            builder3.SetSetMethod(builder5);
            object[] propertyValues = new object[] { r.Name, childFieldName, string.Join(",", (from c in r.ChildCols select c.PropertyName).ToArray<string>()), string.Join(",", (from c in r.ParentCols select c.PropertyName).ToArray<string>()), true };
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(typeof(AssociationAttribute).GetConstructor(new Type[0]), new object[0], new PropertyInfo[] { typeof(AssociationAttribute).GetProperty("Name"), typeof(AssociationAttribute).GetProperty("Storage"), typeof(AssociationAttribute).GetProperty("ThisKey"), typeof(AssociationAttribute).GetProperty("OtherKey"), typeof(AssociationAttribute).GetProperty("IsForeignKey") }, propertyValues);
            builder3.SetCustomAttribute(customBuilder);
        }

        private void EmitStoredProc(TypeBuilder dcType, Action<ILGenerator> dcFieldLoader, SchemaObject procInfo, string objectPrefix, bool useOptional)
        {
            Func<Parameter, Type> func;
            if (useOptional)
            {
                func = p => typeof(Optional<>).MakeGenericType(new Type[] { p.ClrType });
            }
            else
            {
                func = p => p.ClrType;
            }
            MethodBuilder builder = dcType.DefineMethod(procInfo.PropertyName, MethodAttributes.Public, typeof(ReturnDataSet), procInfo.Parameters.Select<Parameter, Type>(func).ToArray<Type>());
            ParameterAttributes attributes = useOptional ? (ParameterAttributes.HasDefault | ParameterAttributes.Optional) : ParameterAttributes.None;
            int arg = 1;
            foreach (Parameter parameter in procInfo.Parameters)
            {
                builder.DefineParameter(arg++, attributes, parameter.ClrName);
            }
            ILGenerator iLGenerator = builder.GetILGenerator();
            LocalBuilder local = iLGenerator.DeclareLocal(typeof(object[]));
            iLGenerator.Emit(OpCodes.Ldarg_0);
            if (dcFieldLoader != null)
            {
                dcFieldLoader(iLGenerator);
            }
            iLGenerator.Emit(OpCodes.Ldstr, string.Concat(new object[] { objectPrefix, '[', procInfo.SchemaName, "].[", procInfo.SqlName, ']' }));
            iLGenerator.Emit(OpCodes.Ldc_I4, procInfo.Parameters.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(object));
            iLGenerator.Emit(OpCodes.Stloc, local);
            arg = 0;
            foreach (Parameter parameter in procInfo.Parameters)
            {
                iLGenerator.Emit(OpCodes.Ldloc, local);
                iLGenerator.Emit(OpCodes.Ldc_I4, arg);
                iLGenerator.Emit(OpCodes.Ldarg, (int) (arg + 1));
                Type clrType = parameter.ClrType;
                if (useOptional)
                {
                    clrType = typeof(Optional<>).MakeGenericType(new Type[] { clrType });
                }
                if (clrType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Box, clrType);
                }
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                arg++;
            }
            iLGenerator.Emit(OpCodes.Ldloc, local);
            MethodInfo meth = typeof(DataContextBase).GetMethod("ExecuteStoredProcedure", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object[]) }, null);
            iLGenerator.Emit(OpCodes.Call, meth);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private Type EmitTableClass(SchemaObject t, string objectPrefix)
        {
            TypeBuilder builder = this._tableTypes[t.DotNetName];
            ConstructorInfo constructor = typeof(TableAttribute).GetConstructor(new Type[0]);
            PropertyInfo property = typeof(TableAttribute).GetProperty("Name");
            string str = BracketsForSql((t.SchemaName == "dbo") ? "" : t.SchemaName);
            if ((objectPrefix.Length > 0) && (str.Length == 0))
            {
                str = "[dbo]";
            }
            string str2 = BracketsForSql(t.SqlName);
            if (str.Length > 0)
            {
                str2 = str + "." + str2;
            }
            str2 = objectPrefix + str2;
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(constructor, new object[0], new PropertyInfo[] { property }, new object[] { str2 });
            builder.SetCustomAttribute(customBuilder);
            ILGenerator iLGenerator = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[0]).GetILGenerator();
            foreach (SqlColumn column in t.Columns.Values)
            {
                this.EmitColumn(builder, column);
            }
            Table table = t as Table;
            if (table != null)
            {
                foreach (Relationship relationship in table.ParentRelations)
                {
                    this.EmitParentRelation(iLGenerator, builder, relationship);
                }
                foreach (Relationship relationship in table.ChildRelations)
                {
                    if (relationship.IsOneToOne)
                    {
                        this.EmitOneToOneOnChildSide(iLGenerator, builder, relationship);
                    }
                    else
                    {
                        this.EmitChildRelation(iLGenerator, builder, relationship);
                    }
                }
            }
            iLGenerator.Emit(OpCodes.Ret);
            return builder;
        }

        private void EmitTableProperty(TypeBuilder dcType, Action<ILGenerator> dcFieldLoader, SchemaObject tableInfo)
        {
            Type type = this._tableTypes[tableInfo.DotNetName];
            string propertyName = tableInfo.PropertyName;
            string name = "get_" + propertyName;
            Type returnType = typeof(Table<>).MakeGenericType(new Type[] { type });
            PropertyBuilder builder = dcType.DefineProperty(propertyName, PropertyAttributes.None, returnType, new Type[0]);
            MethodBuilder mdBuilder = dcType.DefineMethod(name, MethodAttributes.SpecialName | MethodAttributes.Public, returnType, new Type[0]);
            MethodInfo meth = typeof(DataContext).GetMethod("GetTable", new Type[0]).MakeGenericMethod(new Type[] { type });
            ILGenerator iLGenerator = mdBuilder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            if (dcFieldLoader != null)
            {
                dcFieldLoader(iLGenerator);
            }
            iLGenerator.Emit(OpCodes.Call, meth);
            iLGenerator.Emit(OpCodes.Ret);
            builder.SetGetMethod(mdBuilder);
        }

        public static void Generate(Database mainSchema, AssemblyName name, string cxString, string ns, string dataContextName)
        {
            string nsPrefix = string.IsNullOrEmpty(ns) ? "" : (ns + ".");
            string fileName = Path.GetFileName(name.CodeBase);
            AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save, Path.GetDirectoryName(name.CodeBase));
            ModuleBuilder module = builder.DefineDynamicModule("MainModule", fileName, false);
            TypeBuilder type = module.DefineType(nsPrefix + dataContextName, TypeAttributes.Public, typeof(DataContextBase));
            EmitDataContextConstructors(type, cxString);
            new EmissionsGenerator(mainSchema).Generate(type, dotNetName => module.DefineType(nsPrefix + dotNetName, TypeAttributes.Public), null, null, null, null, null, mainSchema.LinkedDatabases.Length == 0);
            foreach (Database database in mainSchema.LinkedDatabases)
            {
                FieldBuilder dcField;
                TypeBuilder nestingContainer = module.DefineType(nsPrefix + database.ClrName + "Types", TypeAttributes.Public);
                TypeBuilder linkedClass = nestingContainer.DefineNestedType("TypedDataContext", TypeAttributes.NestedPublic);
                PopulateLinkedClass(linkedClass, type, database.ClrName, out dcField);
                new EmissionsGenerator(database).Generate(linkedClass, dotNetName => nestingContainer.DefineNestedType(dotNetName, TypeAttributes.NestedPublic), ilGen => ilGen.Emit(OpCodes.Ldfld, dcField), type, database.ClrName, database.ServerName, database.CatalogName, true);
                nestingContainer.CreateType();
            }
            type.CreateType();
            builder.Save(fileName);
        }

        private void Generate(TypeBuilder dcType, Func<string, TypeBuilder> tableTypeCreator, Action<ILGenerator> dcFieldLoader, TypeBuilder parentDC, string clrTypeNamePrefix, string serverName, string dbName, bool createDC)
        {
            this._tableTypes.Clear();
            foreach (SchemaObject obj2 in this.Schema.Objects.Values)
            {
                if (obj2.NeedsCustomType)
                {
                    this._tableTypes.Add(obj2.DotNetName, tableTypeCreator(obj2.DotNetName));
                }
            }
            foreach (Table table in this.Schema.Objects.Values.OfType<Table>())
            {
                TypeBuilder builder = this._tableTypes[table.DotNetName];
                foreach (Relationship relationship in table.ParentRelations)
                {
                    TypeBuilder returnType = this._tableTypes[relationship.ParentTable.DotNetName];
                    this._entityRefProps[relationship] = builder.DefineProperty(relationship.PropNameForChild, PropertyAttributes.HasDefault, returnType, null);
                    MethodBuilder builder3 = builder.DefineMethod("set_" + relationship.PropNameForChild, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, typeof(void), new Type[] { returnType });
                    this._entityRefPropSetMethods[relationship] = builder3;
                }
            }
            string objectPrefix = "";
            if (!(string.IsNullOrEmpty(dbName) || (this.Schema.SystemSchema != null)))
            {
                objectPrefix = BracketsForSql(dbName) + ".";
                if (!string.IsNullOrEmpty(serverName))
                {
                    objectPrefix = BracketsForSql(serverName) + "." + objectPrefix;
                }
            }
            foreach (SchemaObject obj2 in this.Schema.Objects.Values)
            {
                if (obj2.NeedsCustomType)
                {
                    this.EmitTableClass(obj2, objectPrefix);
                }
            }
            this.EmitDataContextMembers(dcType, dcFieldLoader, parentDC, clrTypeNamePrefix, objectPrefix);
            HashSet<TypeBuilder> source = new HashSet<TypeBuilder>();
            ResolveEventHandler handler = delegate (object o, ResolveEventArgs args) {
                TypeBuilder builder;
                string name = args.Name;
                if (name.Contains<char>('.'))
                {
                    name = name.Split(new char[] { '.' }).Last<string>();
                }
                if (!this._tableTypes.TryGetValue(name, out builder))
                {
                    return null;
                }
                if (builder == null)
                {
                    return null;
                }
                Type type = builder.CreateType();
                if (type == null)
                {
                    return null;
                }
                return type.Assembly;
            };
            AppDomain.CurrentDomain.TypeResolve += handler;
            try
            {
                TypeBuilder current;
                if (this._tableTypes.Any<KeyValuePair<string, TypeBuilder>>(t => t.Value.IsNested))
                {
                    dcType.CreateType();
                }
                using (Dictionary<string, TypeBuilder>.ValueCollection.Enumerator enumerator4 = this._tableTypes.Values.GetEnumerator())
                {
                    while (enumerator4.MoveNext())
                    {
                        current = enumerator4.Current;
                        try
                        {
                            current.CreateType();
                            continue;
                        }
                        catch
                        {
                            source.Add(current);
                            continue;
                        }
                    }
                }
                int num = 0;
            Label_02D8:
                if (num >= 3)
                {
                    goto Label_0323;
                }
                TypeBuilder[] builderArray = source.ToArray<TypeBuilder>();
                int index = 0;
            Label_02EB:
                if (index < builderArray.Length)
                {
                    current = builderArray[index];
                    try
                    {
                        current.CreateType();
                        source.Remove(current);
                        goto Label_031B;
                    }
                    catch
                    {
                        goto Label_031B;
                    }
                }
                num++;
                goto Label_02D8;
            Label_031B:
                index++;
                goto Label_02EB;
            Label_0323:
                builderArray = source.ToArray<TypeBuilder>();
                for (index = 0; index < builderArray.Length; index++)
                {
                    builderArray[index].CreateType();
                }
                if (createDC)
                {
                    dcType.CreateType();
                }
            }
            finally
            {
                AppDomain.CurrentDomain.TypeResolve -= handler;
            }
        }

        private static void PopulateLinkedClass(TypeBuilder linkedClass, TypeBuilder dcType, string propName, out FieldBuilder dcField)
        {
            ConstructorBuilder con = linkedClass.DefineConstructor(MethodAttributes.Assembly, CallingConventions.Standard, new Type[] { typeof(DataContextBase) });
            dcField = linkedClass.DefineField("_dc", typeof(DataContextBase), FieldAttributes.Private);
            ILGenerator iLGenerator = con.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Stfld, dcField);
            iLGenerator.Emit(OpCodes.Ret);
            PropertyBuilder builder2 = dcType.DefineProperty(propName, PropertyAttributes.None, linkedClass, new Type[0]);
            MethodBuilder mdBuilder = dcType.DefineMethod("get_" + propName, MethodAttributes.SpecialName | MethodAttributes.Public, linkedClass, new Type[0]);
            ILGenerator generator2 = mdBuilder.GetILGenerator();
            generator2.Emit(OpCodes.Ldarg_0);
            generator2.Emit(OpCodes.Newobj, con);
            generator2.Emit(OpCodes.Ret);
            builder2.SetGetMethod(mdBuilder);
        }

        private class FunctionGenerator
        {
            public readonly TypeBuilder DCType;
            public readonly EmissionsGenerator EGen;
            public readonly SchemaObject FunctionInfo;
            public readonly MethodBuilder Method;
            public readonly Type ResultType;

            public FunctionGenerator(EmissionsGenerator egen, TypeBuilder dcType, SchemaObject funcInfo, string methodName, MethodAttributes accessibility)
            {
                Type resultType;
                this.EGen = egen;
                this.DCType = dcType;
                this.FunctionInfo = funcInfo;
                if (funcInfo is TableFunction)
                {
                    this.ResultType = egen._tableTypes[funcInfo.DotNetName];
                }
                else
                {
                    if (funcInfo.ReturnInfo == null)
                    {
                        return;
                    }
                    this.ResultType = funcInfo.ReturnInfo.ClrType;
                }
                if (funcInfo is TableFunction)
                {
                    resultType = typeof(IQueryable<>).MakeGenericType(new Type[] { this.ResultType });
                }
                else
                {
                    resultType = this.ResultType;
                }
                this.Method = dcType.DefineMethod(methodName, accessibility, resultType, (from p in funcInfo.Parameters select p.ClrType).ToArray<Type>());
            }

            public void AddAttributes(string objectPrefix)
            {
                Type type = typeof(FunctionAttribute);
                CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(type.GetConstructor(Type.EmptyTypes), new object[0], new PropertyInfo[] { type.GetProperty("Name"), type.GetProperty("IsComposable") }, new object[] { string.Concat(new object[] { objectPrefix, '[', this.FunctionInfo.SchemaName, "].[", this.FunctionInfo.SqlName, ']' }), true });
                this.Method.SetCustomAttribute(customBuilder);
                int num = 1;
                foreach (Parameter parameter in this.FunctionInfo.Parameters)
                {
                    ParameterBuilder builder2 = this.Method.DefineParameter(num++, ParameterAttributes.None, parameter.ClrName);
                    Type type2 = typeof(ParameterAttribute);
                    CustomAttributeBuilder builder3 = new CustomAttributeBuilder(type2.GetConstructor(Type.EmptyTypes), new object[0], new PropertyInfo[] { type2.GetProperty("Name"), type2.GetProperty("DbType") }, new object[] { parameter.ParamName.Replace("@", ""), parameter.ParamDbType.Name });
                    builder2.SetCustomAttribute(builder3);
                }
            }
        }
    }
}

