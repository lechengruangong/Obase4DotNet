/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于NewExpression的视图构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:11:47
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Common;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     基于NewExpression的视图构造器。
    /// </summary>
    public class NewExpressionBasedBuilder : ITypeViewBuilder
    {
        /// <summary>
        ///     构造类型视图。
        /// </summary>
        /// <param name="viewExp">视图表达式。</param>
        /// <param name="source">视图源。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="sourcePara">视图表达式中代表视图源的形式参数。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public TypeView Build(Expression viewExp, StructuralType source, ObjectDataModel model,
            ParameterExpression sourcePara,
            params ParameterBinding[] paraBindings)
        {
            var typeView = new TypeView(source, viewExp.Type, sourcePara);
            if (!(viewExp is NewExpression newExp)) throw new ArgumentException($"构造类型视图时，表达式（{viewExp}）不合法。");

            var constructorInfo = newExp.Constructor;
            var paraInfos = constructorInfo.GetParameters();

            //创建LambdaExpression
            var parameters = paraInfos.Select(p => Expression.Parameter(p.ParameterType)).ToArray();
            var body = Expression.New(constructorInfo, parameters.Cast<Expression>());
            var lambda = Expression.Lambda(body, parameters);
            var delegateFunc = lambda.Compile();

            var constructor = (InstanceConstructor)InstanceConstructor.Create(delegateFunc);
            typeView.Constructor = constructor;
            var adder = new ViewElementAdder(typeView, model);

            //加入Member的初始化
            for (var i = 0; i < newExp.Members?.Count; i++)
            {
                var member = newExp.Members.Count > i ? newExp.Members[i] : null;
                var arg = newExp.Arguments.Count > i ? newExp.Arguments[i] : null;
                var paraInfo = paraInfos.Length > i ? paraInfos[i] : null;
                if (member == null || arg == null || paraInfo == null) continue;
                adder.AddElement(newExp.Members[i], newExp.Arguments[i], paraBindings);

                object Convertor(object dbVal)
                {
                    return Utils.ConvertDbValue(dbVal, arg.Type);
                }

                constructor.SetParameter(newExp.Members[i].Name, paraInfos[i].Name, Convertor);
            }

            if (paraInfos.Length > 0)
                //加入构造函数参数
                for (var i = 0; i < paraInfos.Length; i++)
                {
                    var arg = newExp.Arguments[i];
                    if (constructor.Parameters != null)
                        if (constructor.Parameters.FirstOrDefault(p => p.Name == paraInfos[i].Name) != null)
                            continue;

                    object Convertor(object dbVal)
                    {
                        return Utils.ConvertDbValue(dbVal, arg.Type);
                    }

                    constructor.SetParameter(paraInfos[i].Name, paraInfos[i].Name, Convertor, newExp.Arguments[i]);
                }


            typeView.AddParameterBinding(paraBindings);
            return typeView;
        }
    }
}