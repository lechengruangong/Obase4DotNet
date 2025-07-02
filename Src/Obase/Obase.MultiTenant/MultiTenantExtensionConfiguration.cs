/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户扩展配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:54:11
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户扩展配置
    /// </summary>
    /// <typeparam name="TObject">要配置多租户的类型</typeparam>
    public class MultiTenantExtensionConfiguration<TObject> : TypeExtensionConfiguration where TObject : class
    {
        /// <summary>
        ///     全局多租户ID
        /// </summary>
        private object _globalTenantId;

        /// <summary>
        ///     是否包含全局Id进行查询
        /// </summary>
        private bool _loadingGlobal;

        /// <summary>
        ///     多租户标记的映射字段
        /// </summary>
        private string _tenantIdField;

        /// <summary>
        ///     多租户标记的属性的名称
        /// </summary>
        private string _tenantIdMark;

        /// <summary>
        ///     多租户的Id类型
        /// </summary>
        private Type _tenantIdType;

        /// <summary>
        ///     获取类型扩展的类型。
        /// </summary>
        public override Type ExtensionType => typeof(MultiTenantExtension);

        /// <summary>
        ///     配置多租户标记的属性的名称
        /// </summary>
        /// <param name="expression">要配置为多租户的属性表达式</param>
        public void HasTenantIdMark(Expression<Func<TObject, string>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess) throw new Exception("请使用成员表达式配置多租户标记的属性的名称");
            //解析表达式
            var member = (MemberExpression)expression.Body;
            //获取实体模型对应类型的属性
            var property = typeof(TObject).GetProperty(member.Member.Name);

            if (property == null)
                throw new ArgumentNullException(nameof(member.Member.Name),
                    $"无法在实体型{typeof(TObject).FullName}内找到到要配置为多租户的属性{member.Member.Name}");

            _tenantIdType = typeof(string);

            _tenantIdMark = member.Member.Name;
        }

        /// <summary>
        ///     配置多租户标记的属性的名称
        /// </summary>
        /// <param name="expression">要配置为多租户的属性表达式</param>
        public void HasTenantIdMark(Expression<Func<TObject, int>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess) throw new Exception("请使用成员表达式配置多租户标记的属性的名称");
            //解析表达式
            var member = (MemberExpression)expression.Body;
            //获取实体模型对应类型的属性
            var property = typeof(TObject).GetProperty(member.Member.Name);

            if (property == null)
                throw new ArgumentNullException(nameof(member.Member.Name),
                    $"无法在实体型{typeof(TObject).FullName}内找到到要配置为多租户的属性{member.Member.Name}");

            _tenantIdType = typeof(int);

            _tenantIdMark = member.Member.Name;
        }

        /// <summary>
        ///     配置多租户标记的属性的名称
        /// </summary>
        /// <param name="expression">要配置为多租户的属性表达式</param>
        public void HasTenantIdMark(Expression<Func<TObject, long>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess) throw new Exception("请使用成员表达式配置多租户标记的属性的名称");
            //解析表达式
            var member = (MemberExpression)expression.Body;
            //获取实体模型对应类型的属性
            var property = typeof(TObject).GetProperty(member.Member.Name);

            if (property == null)
                throw new ArgumentNullException(nameof(member.Member.Name),
                    $"无法在实体型{typeof(TObject).FullName}内找到到要配置为多租户的属性{member.Member.Name}");

            _tenantIdType = typeof(long);

            _tenantIdMark = member.Member.Name;
        }

        /// <summary>
        ///     配置多租户标记的属性的名称
        /// </summary>
        /// <param name="expression">要配置为多租户的属性表达式</param>
        public void HasTenantIdMark(Expression<Func<TObject, Guid>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess) throw new Exception("请使用成员表达式配置多租户标记的属性的名称");
            //解析表达式
            var member = (MemberExpression)expression.Body;
            //获取实体模型对应类型的属性
            var property = typeof(TObject).GetProperty(member.Member.Name);

            if (property == null)
                throw new ArgumentNullException(nameof(member.Member.Name),
                    $"无法在实体型{typeof(TObject).FullName}内找到到要配置为多租户的属性{member.Member.Name}");

            _tenantIdType = typeof(Guid);

            _tenantIdMark = member.Member.Name;
        }

        /// <summary>
        ///     配置多租户的映射字段
        /// </summary>
        /// <param name="tenantIdField">映射字段</param>
        /// <param name="tenantIdType">字段类型string,int,long,Guid类型中的一种</param>
        public void HasTenantIdField(string tenantIdField, Type tenantIdType)
        {
            if (tenantIdType != typeof(int) && tenantIdType != typeof(long) && tenantIdType != typeof(string) &&
                tenantIdType != typeof(Guid))
                throw new ArgumentException("多租户主键属性必须为string,int,long,Guid类型中的一种");

            _tenantIdField = tenantIdField;

            //已使用Mark配置 此处忽略
            if (string.IsNullOrEmpty(_tenantIdMark))
                _tenantIdType = tenantIdType;
        }

        /// <summary>
        ///     设置全局租户ID
        ///     会同时启用包含全局租户ID查询
        /// </summary>
        /// <param name="tenantId">全局的多租户ID</param>
        public void HasGlobalTenantId(object tenantId)
        {
            var tenantIdType = tenantId.GetType();

            if (_tenantIdType == null)
                throw new ArgumentException("需要先设置多租户主键属性.");

            if (tenantIdType != typeof(int) && tenantIdType != typeof(long) && tenantIdType != typeof(string) &&
                tenantIdType != typeof(Guid))
                throw new ArgumentException("多租户主键属性必须为string,int,long,Guid类型中的一种");

            if (tenantIdType != _tenantIdType)
                throw new ArgumentException("多租户主键属性与全局租户ID值类型不符.");

            _globalTenantId = tenantId;
            _loadingGlobal = true;
        }

        /// <summary>
        ///     设置是否启用包含全局租户ID查询
        /// </summary>
        /// <param name="loadingGlobal">是否要包含全局租户ID的查询</param>
        public void HasLoadingGlobal(bool loadingGlobal)
        {
            _loadingGlobal = loadingGlobal;
        }

        /// <summary>
        ///     根据配置元数据生成类型扩展实例。
        ///     实施说明
        ///     寄存生成结果，避免重复生成。
        /// </summary>
        /// <returns>生成的类型扩展实例。</returns>
        public override TypeExtension MakeExtension()
        {
            if (string.IsNullOrEmpty(_tenantIdMark) && string.IsNullOrEmpty(_tenantIdField))
                throw new ArgumentException("多租户标记TenantIdMark和多租户字段TenantIdField不能同时为空");

            var extension = new MultiTenantExtension
            {
                TenantIdMark = _tenantIdMark,
                TenantIdField = _tenantIdField,
                TenantIdType = _tenantIdType,
                GlobalTenantId = _globalTenantId,
                LoadingGlobal = _loadingGlobal
            };

            return extension;
        }
    }
}