/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：平展后的或因子.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 12:14:01
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示平展筛选条件得到的或因子。
    ///     条件平展
    ///     将条件表示为深度为2的树，其中，根节点为或运算，有多个子节点，每个子节点表示一个运算数，称为或因子。一个或因子可以是一个非逻辑运算的布尔表达式，也可以是多个非逻
    ///     辑运算布尔表达式的连续“与”运算。
    /// </summary>
    public class OrFactor
    {
        /// <summary>
        ///     或因子项。一个或因子等价于所有项的连续与运算。
        /// </summary>
        private readonly Expression[] _items;

        /// <summary>
        ///     在表达式中代表查询源的形式参数。
        /// </summary>
        private readonly ParameterExpression _sourceParameter;

        /// <summary>
        ///     查询源类型。
        /// </summary>
        private readonly ReferringType _sourceType;

        /// <summary>
        ///     基础因子。
        /// </summary>
        private OrFactor _baseFactor;

        /// <summary>
        ///     校验因子。
        /// </summary>
        private OrFactor _checkFactor;

        /// <summary>
        ///     该值指示或因子是否为异构的
        /// </summary>
        private bool? _heterogeneous;

        /// <summary>
        ///     初始化OrFactor类的新实例。
        /// </summary>
        /// <param name="items">或因子项。</param>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="sourcePara">表达式中代表查询源的形参。</param>
        public OrFactor(Expression[] items, ReferringType sourceType, ParameterExpression sourcePara)
        {
            _items = items;
            _sourceType = sourceType;
            _sourceParameter = sourcePara;
        }

        /// <summary>
        ///     获取或因子项。
        /// </summary>
        public Expression[] Items => _items;

        /// <summary>
        ///     获取实施极限分解后的基础因子。
        /// </summary>
        /// <returns>
        ///     极限分解得出的基础因子。如果所有项均为异构的，返回null。
        ///     警告
        ///     不会检测或因子是否为异构，对于同构或因子，将生成其副本作为基础因子。强烈建议调用前确保或因子是异构的。
        /// </returns>
        public OrFactor BaseFactor => _baseFactor;

        /// <summary>
        ///     获取实施极限分解后的校验因子。
        /// </summary>
        /// <returns>
        ///     极限分解得出的校验因子。如果所有项均为同构的，返回null。
        ///     警告
        ///     不会检测或因子是否为异构，对于同构或因子，将生成其副本作为基础因子，然后返回null。强烈建议调用前确保或因子是异构的。
        /// </returns>
        public OrFactor CheckFactor => _checkFactor;

        /// <summary>
        ///     获取查询源类型。
        /// </summary>
        public ReferringType SourceType => _sourceType;

        /// <summary>
        ///     获取在表达式中代表查询源的形式参数。
        /// </summary>
        public ParameterExpression SourceParameter => _sourceParameter;

        /// <summary>
        ///     获取一个值，该值指示或因子是否为异构的。
        ///     实施说明
        ///     对每个或因子项，提取成员表达式然后从中抽取关联树，如果关联树是异构的则该项是异构的。只要一个项是异构的，整个或因子就是异构的。
        ///     寄存判定结果，避免重复判定。
        /// </summary>
        public bool Heterogeneous
        {
            get
            {
                //没值 对每个项进行运算
                if (_heterogeneous == null) DecomposeExtremely();

                return _heterogeneous != null && _heterogeneous.Value;
            }
        }

        /// <summary>
        ///     将当前或因子与指定的或因子合并，生成一个新的或因子。
        /// </summary>
        /// <param name="other">参与合并的或因子。</param>
        public OrFactor And(OrFactor other)
        {
            if (other._sourceType != _sourceType)
                throw new InvalidOperationException($"要合并的查询源为{other._sourceType},与本身查询源{_sourceType}不符,无法合并.");

            if (other._sourceParameter != _sourceParameter)
                throw new InvalidOperationException("要合并的形式参数与本身形式参数不符,无法合并.");
            //取各自的项 合并
            var expressions = new List<Expression>();
            expressions.AddRange(_items);
            expressions.AddRange(other._items);

            return new OrFactor(expressions.ToArray(), _sourceType, _sourceParameter);
        }

        /// <summary>
        ///     用表达式表示或因子。
        /// </summary>
        public LambdaExpression ToLambda()
        {
            //每个都是或运算
            var exp = Items.Aggregate<Expression, Expression>(null,
                (current, item) => current == null ? item : Expression.Or(current, item));

            var parameter = Expression.Parameter(_sourceType.ClrType, "p");
            //形如(p) => p.Item1 || p.Item2 || ... || p.ItemN
            var result = Expression.Lambda(exp ?? throw new InvalidOperationException("无法为空的或因子数组构造表达式"), parameter);

            return result;
        }


        /// <summary>
        ///     将当前或因子作为校验因子，生成检验视图。
        /// </summary>
        /// <param name="checkAttrs"></param>
        public TypeView GenerateCheckView(out ViewAttribute[] checkAttrs)
        {
            //注意:此处图对应为执行映射/Query/查询链/生成校验视图 图中的_parameterbindings所有引用都传了null

            //用于申请隐含类型的字段描述器
            var filedDescriptors = new List<FieldDescriptor>();

            FieldDescriptor[] checkFields = null;
            if (_checkFactor?.Items != null)
            {
                //构造校验属性描述
                checkFields = _checkFactor?.Items.Select(p =>
                {
                    if (p is BinaryExpression binary) return new FieldDescriptor(binary.Left);

                    return null;
                }).ToArray();
                if (checkFields != null) filedDescriptors.AddRange(checkFields.Where(p => p != null));
            }

            var filterAttrs = _sourceType.GetFilterKey();
            FieldDescriptor[] filterFields = null;
            if (filterAttrs != null)
            {
                //构造过滤属性描述
                filterFields = filterAttrs.Select(p => new FieldDescriptor(p.DataType)).ToArray();
                filedDescriptors.AddRange(filterFields);
            }

            //申请隐含类型
            var impliedType =
                ImpliedTypeManager.Current.ApplyType(_sourceType.ClrType, filedDescriptors.ToArray(),
                    new IdentityArray(_sourceType.FullName));
            //视图
            var result = new TypeView(_sourceType, impliedType, _sourceParameter) { IsDecomposeExtremelyResult = true };

            List<ViewAttribute> checkAttributes = null;
            //添加校验属性
            if (checkFields != null)
            {
                checkAttributes = new List<ViewAttribute>();
                var adder = new ViewElementAdder(result, _sourceType.Model);
                foreach (var checkField in checkFields)
                {
                    //添加
                    var element = adder.AddElement(impliedType.GetMember(checkField.Name)[0],
                        checkField.ValueExpression);
                    if (element is ViewAttribute viewAttribute)
                        checkAttributes.Add(viewAttribute);
                }
            }

            //添加过滤属性
            if (filterAttrs != null)
                for (var i = 0; i < filterAttrs.Length; i++)
                {
                    var viewAttribute = new ViewAttribute(filterFields[i].Name, filterAttrs[i]);
                    result.AddElement(viewAttribute);
                }

            checkAttrs = checkAttributes?.ToArray();

            result.GenerateType();
            return result;
        }

        /// <summary>
        ///     对或因子实施极限分解。
        ///     实施说明
        ///     如果已执行过分解操作（定义一个寄存器），则不再执行。
        /// </summary>
        private void DecomposeExtremely()
        {
            //已分解过
            if (_baseFactor != null || _checkFactor != null || _heterogeneous.HasValue) return;

            var result = false;
            //异构的和不异构的
            var heterogItem = new List<Expression>();
            var homogItem = new List<Expression>();
            foreach (var item in _items)
            {
                //提取成员表达式
                var memberExpressions = new MemberExpressionExtractor(new SubTreeEvaluator(item)).ExtractMember(
                    item);
                foreach (var memberExpression in memberExpressions)
                {
                    //判断是否异构
                    var associationTree = memberExpression.ExtractAssociation(_sourceType.Model);
                    var predicater =
                        new AssociationTreeHeterogeneityPredicater(new StorageHeterogeneityPredicationProvider());
                    var itemHeter = associationTree.Accept(predicater);
                    if (itemHeter)
                    {
                        result = true;
                        break;
                    }

                    result = false;
                }

                //加入不同的集合
                if (result)
                    heterogItem.Add(item);
                else
                    homogItem.Add(item);
            }

            //有异构项 则整体为异构的
            _heterogeneous = heterogItem.Count > 0;
            _checkFactor = heterogItem.Count == 0
                ? null
                : new OrFactor(heterogItem.ToArray(), _sourceType, _sourceParameter);
            _baseFactor = homogItem.Count == 0
                ? null
                : new OrFactor(homogItem.ToArray(), _sourceType, _sourceParameter);
        }

        /// <summary>
        ///     用表达式表示多个或因子的连续或运算。
        /// </summary>
        /// <param name="orFactors">要处理的或因子</param>
        public static LambdaExpression ToLambda(OrFactor[] orFactors)
        {
            //每个都转换成lambda
            var orLambdaExpressionList = orFactors.Select(orFactor => orFactor.ToLambda()).ToList();

            //每个都是或运算
            Expression exp = null;
            foreach (var orExpression in orLambdaExpressionList)
                if (exp == null)
                    exp = orExpression;
                else
                    exp = Expression.Or(exp, orExpression);

            var parameter = Expression.Parameter(orFactors[0].SourceType.ClrType, "p");
            //形如(p) => p.Item1 || p.Item2 || ... || p.ItemN
            var result = Expression.Lambda(exp ?? throw new InvalidOperationException("无法为空的或因子数组构造表达式"), parameter);

            return result;
        }
    }
}