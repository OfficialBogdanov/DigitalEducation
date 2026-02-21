using System;

namespace DigitalEducation
{
    public interface IPage
    {
    }

    public interface IPageFactory
    {
        T CreatePage<T>() where T : IPage, new();
        IPage CreatePage(Type pageType);
    }

    public class DefaultPageFactory : IPageFactory
    {
        public T CreatePage<T>() where T : IPage, new() => new T();

        public IPage CreatePage(Type pageType)
        {
            if (!typeof(IPage).IsAssignableFrom(pageType))
                throw new ArgumentException("Тип должен реализовывать IPage");
            return Activator.CreateInstance(pageType) as IPage;
        }
    }
}