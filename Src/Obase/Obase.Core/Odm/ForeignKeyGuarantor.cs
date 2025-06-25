/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：提供确保所有的外键属性都已定义的机制,提供执行保证方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-20 11:37:02
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     提供确保所有的外键属性都已定义的机制。
    ///     说明
    ///     检查需要定义的外键属性是否已存在，如果不存在则自动定义，并将定义的属性追加至模型类型。
    ///     对于关联型，检查其自身的外键；对于实体型，检查其作为关联端参与的关联型的外键。
    /// </summary>
    public abstract class ForeignKeyGuarantor
    {
        /// <summary>
        ///     执行保证。
        /// </summary>
        /// <param name="objType">确保其定义外键的对象类型。</param>
        /// <param name="returnEnd">要返回其外键的关联端。</param>
        public Attribute[] Guarantee(ObjectType objType, AssociationEnd returnEnd)
        {
            //获取需要定义的外键属性
            var attrs = Utils.GetDefinedForeignAttributes(objType, returnEnd, out var returnKey);

            //有属性 定义所缺的属性
            if (attrs.Count > 0)
            {
                //定义属性
                DefineMissing(attrs.ToArray(), objType);
                //检查是否有属性没有成功定义
                foreach (var attribute in attrs)
                    // 属性没有设值器或取值器，则认为没有成功定义抛出异常
                    if (attribute.ValueSetter == null || attribute.ValueGetter == null)
                        throw new ForeignKeyGuarantingException("构造外键时错误,没有为外键设置设值器或取值器");
            }

            return returnKey.ToArray();
        }

        /// <summary>
        ///     在外键属性缺失的情况下定义所缺的属性。
        /// </summary>
        /// <param name="attrs">要定义的外键属性。</param>
        /// <param name="objType">要定义属性的类型。</param>
        protected abstract void DefineMissing(Attribute[] attrs, ObjectType objType);
    }
}