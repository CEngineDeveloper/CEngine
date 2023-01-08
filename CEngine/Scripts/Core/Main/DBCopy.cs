using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CYM
{
    public class DBCopy<TConfig, TData>
        where TConfig:class,new()
        where TData:class,new()
    {
        static DBCopy()
        {
            _confToData = ConfToData();
            _dataToConf = DataToConf();
        }
        private static Action<TConfig, TData> _confToData; 
        private static Action<TData, TConfig> _dataToConf;

        private static Action<TConfig, TData> ConfToData() 
        {
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type typeConfig = typeof(TConfig);
            Type typeData = typeof(TData);
            ParameterExpression tConfig = Expression.Parameter(typeConfig, "TConfig"),tData = Expression.Parameter(typeData, "TData");
            List<BinaryExpression> LsExp = new List<BinaryExpression>();
            foreach (FieldInfo field in typeData.GetFields(flag))
            {
                var cAttr = typeConfig.GetProperty(field.Name, flag);
                if (cAttr != null)
                {
                    MemberExpression configMemb = Expression.Property(tConfig, field.Name);
                    MemberExpression dataMemb = Expression.Field(tData, field.Name);
                    BinaryExpression setValue = Expression.Assign(dataMemb, configMemb);
                    LsExp.Add(setValue);
                }
            }
            BlockExpression body = Expression.Block(typeof(void), LsExp);
            return Expression.Lambda<Action<TConfig, TData>>(body, tConfig, tData).Compile();
        }
        private static Action<TData, TConfig> DataToConf()
        {
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type typeConfig = typeof(TConfig);
            Type typeData = typeof(TData);
            ParameterExpression tConfig = Expression.Parameter(typeConfig, "TConfig"), tData = Expression.Parameter(typeData, "TData");
            List<BinaryExpression> LsExp = new List<BinaryExpression>();
            foreach (FieldInfo field in typeData.GetFields(flag))
            {
                var cAttr = typeConfig.GetProperty(field.Name, flag);
                if (cAttr != null)
                {
                    MemberExpression configMemb = Expression.Property(tConfig, field.Name);
                    MemberExpression dataMemb = Expression.Field(tData, field.Name);
                    BinaryExpression setValue = Expression.Assign(configMemb, dataMemb);
                    LsExp.Add(setValue);
                }
            }
            BlockExpression body = Expression.Block(typeof(void), LsExp);
            return Expression.Lambda<Action<TData, TConfig>>(body,tData, tConfig).Compile();
        }

        public static void CopyToData(TConfig config,TData data,bool safe=false)
        {
            if (safe)
            {
                if (config == null || data == null)
                    return;
            }
            else
            {
                if (config == null) config = new TConfig();
                if (data == null) data = new TData();
            }

            _confToData(config, data);
        }
        public static void CopyToConfig(TData data, TConfig config, bool safe = false)
        {
            if (safe)
            {
                if (config == null || data == null)
                    return;
            }
            else
            {
                if (config == null) config = new TConfig();
                if (data == null) data = new TData();
            }

            _dataToConf(data, config);
        }
    }
}
