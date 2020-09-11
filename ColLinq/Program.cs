using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq.Extensions;

namespace ColLinq
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            #region Linq

            // SQL 风格：语法和 SQL 相似，部分复杂查询用 SQL 风格语义会更清晰明了，比如 SelectMany 和 Join查询。SQL 风格的可读性有绝对优势，但不支持全部标准 Linq 函数，不支持自定义函数。纯粹的语法糖。
            // 函数风格：以 C# 扩展方法的方式实现，扩展方法既可以是标准库提供的也可以是自己实现的，完全的原生编程风格，编译后的代码都是函数调用的。支持全部标准 Linq 函数和任何自定义函数。
            // 使用 Linq 查询的前提是对象必须是一个 IEnumerable 集合。
            // Linq 查询大多是都是链式查询，即操作的数据源是 IEnumerable<T1> 类型，返回的是 IEnumerable<T2> 类型，T1 和 T2 可以相同，也可以不相同。
            // Linq To Object 查询内存集合，直接把查询编译成 .Net 代码执行
            // Linq To Provider 查询自定义数据源，由开发者提供相应数据源的 provider 并翻译和执行自定义查询。例如 Json Xml 等都可以作为 Provider 对应的数据源，数据源对应的 Linq 查询叫 Linq To <数据源>，比如 Linq To Xml

            #region 定义变量

            // 使用 let 定义本地变量
            // let variable_name = 22;

            int[] numbers = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var result = from number in numbers
                         let average = numbers.Average()
                         let squared = Math.Pow(number, 2)
                         where squared > average
                         select number;

            #endregion 定义变量

            #region Selete

            // 其中的 Select 方法接收的参数用的最多的是 Func<TSource,TResult>, 它还可以接收 Func<TSource,int,TResult> 参数。
            var collectionWithRowNumber = numbers
                .Select((item, index) => new
                {
                    Item = item,
                    RowNumber = index
                })
                .ToList();

            #endregion Selete

            #region First、Last、Single

            // First、FirstOrDefault、Last、LastOrDefault、Single、SingleOrDefault 是快速查询集合中的第一个或最后一个元素的方法。
            // 如果集合时空的，First、Last 和 Single 都会报错，如果使其不报错而在空集合时使用默认值可以使用 FirstOrDefault、LastOrDefault、SingleOrDefault。
            // Single / SingleOrDefault 和其它方法的区别是，他限定查询结构只有一个元素，如果查询结果集合中包含多个元素时会报错。

            new[] { "a", "b" }.First(x => x.Equals("b")); // 返回 b
            //new[] { "a", "b" }.First(x => x.Equals("c"));   // 抛出 InvalidOperationException 异常
            new[] { "a", "b" }.FirstOrDefault(x => x.Equals("c")); // 返回 null

            new[] { "a", "b" }.Single(x => x.Equals("b")); // 返回 b
            //new[] { "a", "b" }.Single(x => x.Equals("c"));    // 抛出 InvalidOperationException 异常
            new[] { "a", "b" }.SingleOrDefault(x => x.Equals("c")); // 返回null
            //new[] { "a", "b" }.Single();      // 抛出InvalidOperationException 集合中有多个元素抛出异常，只有一个元素时才不会抛出异常

            #endregion First、Last、Single

            #region Except 取差集

            // Linq 的 Except 方法用来取差集，即取出集合中与另一个集合所有元素不同的元素

            int[] first = { 1, 2, 3, 4 };
            int[] second = { 0, 2, 3, 5 };
            IEnumerable<int> exceptResult = first.Except(second);
            // exceptResult = {1,4};

            // Except 方法会去除重复元素
            int[] third = { 1, 1, 1, 2, 3, 4 };
            IEnumerable<int> secondExceptResult = first.Except(second);
            // exceptResult = {1,4};

            // 对于简单类型（int，float，string 等） 使用Except 很简单，但对于自定义类型（复合类型）的Object 如何使用 Except？
            // 此时 需要将自定义类型实现 IEquatable<T> 接口

            #endregion Except 取差集

            #region SelectMany 集合降维

            // SelectMany 可以把多维集合降维，比如把二维的集合平铺成一个一维的集合。
            var selectManyCollection = new int[][]
            {
                new[] {1, 2, 3},
                new[] {4, 5, 6}
            };
            var selectManyResult = selectManyCollection.SelectMany(x => x);
            // selectManyResult = [1,2,3,4,5,6]

            var departments = new[]
            {
                new SelectManyDepartment()
                {
                    SelectManyEmployees = new[]
                    {
                        new SelectManyEmployee {Name = "Bob"},
                        new SelectManyEmployee {Name = "Jack"}
                    }
                },
                new SelectManyDepartment()
                {
                    SelectManyEmployees = new[]
                    {
                        new SelectManyEmployee {Name = "Jim"},
                        new SelectManyEmployee {Name = "John"}
                    }
                }
            };

            // 获取各个部门的员工，查询到一个结果集中 List<SelectManyEmployee>
            var names = departments.SelectMany(c => c.SelectManyEmployees);
            //foreach (var name in names)
            //{
            //    Console.WriteLine(name);
            //}

            #endregion SelectMany 集合降维

            #region SelectMany 笛卡尔积运算

            // SelectMany 不光适用于单个包含多维集合对象的降维，也适用于多个集合之前的两两相互操作。
            var list1 = new List<string> { "a1", "a2" };
            var list2 = new List<string> { "b1", "b2", "b3" };

            // 两两组合
            var resultAB = new List<string>();
            foreach (var s1 in list1)
                foreach (var s2 in list2)
                    resultAB.Add($"{s1}{s2}");

            // 使用 SelectMany 实现
            resultAB = list1.SelectMany(c => list2.Select(y => $"{c}{y}")).ToList();

            #endregion SelectMany 笛卡尔积运算

            #region SelectMany 笛卡尔积运算---N个集合

            // 需要递归运算
            var arrList = new List<string[]>
            {
                new[] {"a1", "a2"},
                new[] {"b1", "b2", "b3"},
                new[] {"c1"}
            };
            Recursion(arrList, 0, new List<string>());

            #endregion SelectMany 笛卡尔积运算---N个集合

            #region Aggregate 聚合

            // Aggregate 扩展方法可以对一个集合依次执行类似累加器的操作，就像滚雪球一样把数据逐步聚集在一起。
            int[] aggregateNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // 第一步，prevSum 取第一个元素的值，即 prevSum = 0 + 1
            // 第二步，把第一步得到的 prevSum 的值加上第二个元素，即 prevSum = prevSum + 2
            // 依此类推，第 i 步把第 i-1 得到的 prevSum 加上第 i 个元素

            int sum = aggregateNumbers.Aggregate((prevSum, current) => prevSum + current);

            string[] stringList = { "Hello", "World", "!" };
            string joinedString = stringList.Aggregate((prev, current) => prev + "" + current);
            // joinedString = "Hello World !"

            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var resultAggregate = items.Aggregate(new { Total = 0, Even = 0, FourthItems = new List<int>() },
                (accum, item) =>
                    new
                    {
                        Total = accum.Total + 1, // 计算集合元素的总数个数
                        Even = accum.Even + (item % 2 == 0 ? 1 : 0), // 计算值为偶数的元素个数
                        FourthItems = (accum.Total + 1) % 4 == 0
                            ? new List<int>(accum.FourthItems) { item }
                            : accum.FourthItems // 收集每 第4个元素
                    });

            // resultAggregate:
            // Total = 12
            // Even = 6
            // FourthItems = [4, 8, 12]

            // 由于匿名类型的属性是只读的，所以在累加的过程都 new 了一个新对象。 如果初始值使用的是自定义类型，那么累加时就不需new 新对象了。

            #endregion Aggregate 聚合

            #region Join 关联查询

            var left = new List<string>() { "a", "b", "c" };
            var right = new List<string>() { "a", "c", "d" };

            #region InnerJoin

            var innerJoinResult = from l in left
                                  join r in right on l equals r
                                  select new { l, r };

            innerJoinResult = left
                .Join(right,
                    l => l,
                    r => r,
                    (l, r) =>
                        new
                        {
                            l,
                            r
                        });

            // result: {"a","a"}
            //         {"c","c"}

            #endregion InnerJoin

            #region LeftJoin

            var leftJoinResult = from l in left
                                 join r in right on l equals r into temp
                                 from t in temp.DefaultIfEmpty()
                                 select new { Left = l, Right = t };

            leftJoinResult = from l in left
                             from r in right.Where(x => x.Equals(l)).DefaultIfEmpty()
                             select new { Left = l, Right = r };

            var le = left
                .GroupJoin(right,
                    l => l,
                    r => r,
                    (l, r) => new
                    {
                        Left = l,
                        Right = r
                    })
                .SelectMany(temp => temp.Right.DefaultIfEmpty(),
                    (l, r) =>
                        new
                        {
                            Left = l,
                            Right = r
                        });

            // result: {"a","a"}
            //         {"b", null}
            //         {"c","c"}

            #endregion LeftJoin

            #region RightJoin

            var rightJoinResult = from r in right
                                  join l in left on r equals l into temp
                                  from t in temp.DefaultIfEmpty()
                                  select new { Left = t, Right = r };
            // result: {"a","a"}
            //         {"c","c"}
            //         {null,"d"}

            #endregion RightJoin

            #region CrossJoin

            var crossJoinResult = from l in left
                                  from r in right
                                  select new { l, r };
            // result: {"a","a"}
            //         {"a","c"}
            //         {"a","d"}
            //         {"b","a"}
            //         {"b","c"}
            //         {"b","d"}
            //         {"c","a"}
            //         {"c","c"}
            //         {"c","d"}

            #endregion CrossJoin

            #region FullOuterJoin

            var leftJoin = from l in left
                           join r in right on l equals r into temp
                           from t in temp.DefaultIfEmpty()
                           select new { First = l, Second = t };

            var rightJoin = from r in right
                            join l in left on r equals l into temp
                            from t in temp.DefaultIfEmpty()
                            select new { First = t, Second = r };

            var fullOuterJoin = leftJoin.Union(rightJoin);

            #endregion FullOuterJoin

            #region 根据多个键关联

            // SQL 中，表与表进行关联查询时 on 条件可以指定多个键的逻辑判断，
            // 用 and 或 or 连接。 但是 Linq 不支持 and 关键字，若要根据多键
            // 关联，需要把要关联的键值分别以相同的属性名放到匿名类中，然后使用
            // equals 比较两个匿名对象是否相等。

            var stringProps = typeof(string).GetProperties();
            var builderProps = typeof(StringBuilder).GetProperties();

            var query =
                from s in stringProps
                join b in builderProps
                    on
                    new { s.Name, s.PropertyType }
                    equals
                    new { b.Name, b.PropertyType }
                select new
                {
                    s.Name,
                    s.PropertyType
                };

            #endregion 根据多个键关联

            #endregion Join 关联查询

            #region Skip&Take分页

            // Skip 扩展方法用来跳过从起始位置开始的指定数量的元素读取集合
            // Take 扩展方法用来从集合中只读取指定数量的元素
            var values = new[] { 5, 4, 3, 2, 1 };
            var skipTwo = values.Skip(2); // {3,2,1}
            var takeThree = values.Take(3); // {5,4,3}
            var skipOneTakeTwo = values.Skip(1).Take(2); // {4,3}
            //int startIndex = (pageNumber - 1) * pageSize;
            //return collection.Skip(startIndex).Take(pageSize);

            #region SkipWhile&TakeWhile 分页

            // SkipWhile 从起始位置开始忽略元素，直到遇到不符合条件的元素则停止忽略，往后就是要查询的结果
            // TakeWhile 从起始位置开始读取元素符合条件的元素，一旦遇到不符合条件的就停止读取，即使后面还有符合条件的也不再读取。
            int[] skipWhileList = { 42, 42, 6, 6, 6, 42 };
            var skipWhileResult = skipWhileList.SkipWhile(i => i == 42);

            // skipWhileResult:6,6,6,42

            int[] takeWhileList = { 1, 10, 40, 50, 44, 70, 4 };
            var takeWhileResult = takeWhileList.TakeWhile(item => item < 50).ToList();
            // takeWhileResult:1,10,40

            // SkipLast(index)：跳过最后两个元素
            // TakeLast(index)：获取最后两个元素
            var takeLast = Enumerable.SkipLast(takeWhileList, 2);

            #endregion SkipWhile&TakeWhile 分页

            #endregion Skip&Take分页

            #region Zip拉链

            // Zip 扩展方法操作的对象是两个集合，它就像拉链一样，根据位置将两个集合中的每个元素依次配对在一起。
            // 其接收的参数是一个 Func 实例，该 Func 实例允许我们成对在处理两个集合中的元素。
            // 如果两个集合中的元素个数不相等，那么多出来的将会被忽略。
            int[] zipNumbers = { 3, 5, 7 };
            string[] words = { "three", "five", "seven", "ignored" };

            IEnumerable<string> zip = zipNumbers.Zip(words, (n, w) => n + "=" + w);

            //3 = three
            //5 = five
            //7 = seven

            #endregion Zip拉链

            #region OfType 和 Cast 类型过滤与转换

            // OfType 用于筛选集合中指定类型的元素，Cast 可以把集合转换为指定类型，但要求源类型必须可以隐式转换为目标类型。
            var item0 = new Foo();
            var item1 = new Foo();
            var item2 = new Bar();
            var item3 = new Bar();

            var ofTypeCollection = new IFoo[] { item0, item1, item2, item3 };
            var foos = ofTypeCollection.OfType<Foo>(); // 获取集合中 类型为 Foo 的对象：item0，item1
            var bars = ofTypeCollection.OfType<Bar>(); // 获取集合中 类型为 Bar 的对象：item2，item3
            var foosAndBars = ofTypeCollection.OfType<IFoo>(); // 获取集合中 类型为 IFoo 的对象，item0，item1，item2，item3

            // 等同于使用 Where
            var foos1 = ofTypeCollection.Where(item => item is Foo);
            var bars1 = ofTypeCollection.Where(item => item is Bar);

            // 将集合隐式转换为 Bar 类型 但源类型必须可以隐式转换为目标类型
            var barsCast = ofTypeCollection.Cast<Bar>(); // 抛出异常 InvalidCastException 因为 item0、item1类型为 Foo不是Bar
            var foosCast = ofTypeCollection.Cast<Foo>(); // InvalidCastException 异常 因为 item2、item3 是 Bar 类型不是 Foo类型
            var foosAndBarsCast = ofTypeCollection.Cast<IFoo>(); // OK 因为所有对象都继承自 IFoo 接口

            #endregion OfType 和 Cast 类型过滤与转换

            #region ToLookup 索引式查找

            // ToLookup 扩展方法返回的是可索引查找的数据结构，它是一个 ILookup 实例，所有元素根据指定的键进行分组并可以按键进行索引。
            string[] array = { "one", "two", "three" };

            // 根据元素字符串长度创建一个查找对象
            // 根据元素字符串长度进行分组
            var lookup = array.ToLookup(item => item.Length);

            // 查找字符串长度为 3 的元素
            var lookupResult = lookup[3];

            // result:one,two

            int[] arrayInt = { 1, 2, 3, 4, 5, 6, 7, 8 };

            // 创建一个奇偶查找（键为 0 和 1）
            var lookupInt = arrayInt.ToLookup(item => item % 2);

            // 查找偶数
            var even = lookupInt[0];
            // even:2,4,6,8

            // 查找奇数
            var odd = lookupInt[1];
            // odd:1,3,5,7

            #endregion ToLookup 索引式查找

            #region Distinct去重

            // Distinct 方法用来去除重复项
            int[] distinctArray = { 1, 2, 3, 4, 2, 5, 3, 2, 1 };

            var distinct = distinctArray.Distinct();

            // 简单类型的集合调用Distinct 方法使用的是默认的比较器，Distinct 方法用此比较器来判断元素是否与其它元素重复，但对于自定义类型要实现去重则需要自定义比较器。

            var people = new List<Person>();
            var peopleDistinct = people.Distinct(new IdEqualityComparer());

            #endregion Distinct去重

            #region ToDictionary 字典转换

            // ToDictionary 扩展方法可以把集合 IEnumerable<TElement> 转换为 Dictionary<Tkey,TValue> 结构的字典，它接受一个 Func<TSource,TKey> 参数用来返回每个元素指定的键与值。

            IEnumerable<SelectManyEmployee> toDictionaryEmployees = new[]
            {
                new SelectManyEmployee {Name = "Bob"},
                new SelectManyEmployee {Name = "Jack"}
            };

            var empDics = toDictionaryEmployees.ToDictionary(c => c.Name);
            // {SelectManyEmployee.Name，SelectManyEmployee }

            toDictionaryEmployees.ToDictionary(c => c.Name, c => c.Name);
            // {SelectManyEmployee.Name，SelectManyEmployee.Name }

            // 键是否区分大小写
            toDictionaryEmployees.ToDictionary(c => c.Name, c => c.Name, StringComparer.InvariantCultureIgnoreCase);

            // 注意，字典类型要求所有键不能重复，所以在使用 ToDictionary 方法时要确保作为字典的键的元素属性不能有重复值，否则会抛出异常。

            #endregion ToDictionary 字典转换

            #region Range 和 Repeat

            // 生成 1-100 的数字，即
            var range = Enumerable.Range(1, 100);

            // 生成三个重复的字符串 'a'，即结果为 ["a","a","a"]
            var repeatedValues = Enumerable.Repeat("a", 3);

            #endregion Range 和 Repeat

            #region Any 和 All

            // Any 用来判断集合中是否存在任一 一个元素符合条件，有一个元素就为true

            // All 用来判断集合中是否所有元素符合条件。   用来判断集合中的所有 元素符合条件，所有元素都符合就为 true

            var anyNumbers = new int[] { 1, 2, 3, 4, 5 };

            //if (source is IIListProvider<TSource> ilistProvider)
            //{
            //    int count = ilistProvider.GetCount(true);
            //    if (count >= 0)
            //        return (uint)count > 0U;
            //}
            //else if (source is ICollection collection)
            //    return (uint)collection.Count > 0U;
            //using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            //    return enumerator.MoveNext();
            bool anyResult = anyNumbers.Any(); // Any 如果是个 元素是个集合，如果Count > 0 就返回true

            // 遍历集合判断集合中是否有 这个元素
            // foreach (TSource source1 in source)
            //{
            //    if (predicate(source1))
            //        return true;
            //}
            anyResult = anyNumbers.Any(c => c == 6);     // false

            // 遍历集合判断集合中是否有不满足条件的元素
            //foreach (TSource source1 in source)
            //{
            //    if (!predicate(source1))
            //        return false;
            //}
            bool allResult = anyNumbers.All(x => x > 0);    // true
            allResult = anyNumbers.All(x => x > 1);     // false    因为有一个 1 不大于1

            #endregion Any 和 All

            #region Concat 和 Union

            // Concat 用来连接两个集合，不会去除重复元素。
            List<int> fooConcat = new List<int> { 1, 2, 3 };
            List<int> barConcat = new List<int> { 3, 4, 5 };

            var resultConcat = Enumerable.Concat(fooConcat, barConcat).ToList();
            resultConcat = fooConcat.Concat(barConcat).ToList();
            // resultConcat : 1,2,3,3,4,5

            // Union 也是用来拼接两个集合，与 Concat 不同的是，它会去除重复项。
            resultConcat = fooConcat.Union(barConcat).ToList();
            // resultConcat : 1,2,3,4,5

            #endregion Concat 和 Union

            #region GroupBy分组

            // GroupBy 扩展方法用来对集合进行分组，下面是一个根据奇偶进行分组的示例
            var groupByList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var groupByResult = groupByList.GroupBy(x => x % 2 == 0);

            #endregion GroupBy分组

            #region DefaultIfEmpty 空替换

            // 在上面的关联查询中我们使用了 DefaultIfEmpty 扩展方法，它表示在没有查询到指定条件的元素时使用元素的默认值替代。
            var chars = new List<string> { "a", "b", "c", "d" };
            chars.Where(s => s.Length > 1).DefaultIfEmpty().First();        // 返回null
            chars.DefaultIfEmpty("N/A").FirstOrDefault();   // 返回 a
            chars.Where(s => s.Length > 1).DefaultIfEmpty("N/A").FirstOrDefault(); // 返回 "N/A"

            #endregion DefaultIfEmpty 空替换

            #region SequenceEqual 集合相等

            // SequenceEqual 扩展方法用于比较集合系列各个相同位置的元素是否相等。
            int[] a = { 1, 2, 3 };
            int[] b1 = { 1, 2, 3 };
            int[] c = { 1, 3, 2 };

            //if (first is ICollection<TSource> sources && second is ICollection<TSource> sources)
            //{
            //    if (sources.Count != sources.Count)
            //        return false;
            //    if (sources is IList<TSource> sourceList && sources is IList<TSource> sourceList)
            //    {
            //        int count = sources.Count;
            //        for (int index = 0; index < count; ++index)
            //        {
            //            if (!comparer.Equals(sourceList[index], sourceList[index]))
            //                return false;
            //        }
            //        return true;
            //    }
            //}

            bool result1 = a.SequenceEqual(b1);  // true
            bool result2 = a.SequenceEqual(c);  // false

            #endregion SequenceEqual 集合相等

            #region Intersect 交集

            int[] intersectA = new[] { 1, 2, 3, 4 };
            int[] intersectB = new[] { 1, 2, 4 };

            var intersectList = intersectB.Intersect(intersectA);

            #endregion Intersect 交集

            Console.ReadKey();

            #endregion Linq

            #region MoreLinq

            // MoreLinq 是 IEnumerable 的扩展方法，支持 LeftJoin、RightJoin 等。

            var userList = new List<User>()
            {
                new User(){ UserID=1, Email="333@qq.com"},
                new User(){ UserID=2, Email="444@qq.com"},
            };

            var orderList = new List<Order>()
            {
                new Order(){ OrderID=1, OrderTitle="订单1", UserID=1},
                new Order(){ OrderID=2, OrderTitle="订单2", UserID=1}
            };

            var moreLinqLeftJoin = userList.LeftJoin(orderList,
                     u => u.UserID,
                    o => o.UserID,
                        u => new { UserId = u.UserID, OrderTitle = default(string) },
                        (u, o) => new { UserId = u.UserID, OrderTitle = o.OrderTitle }
            );

            #endregion MoreLinq

            Console.WriteLine("Hello World!");
        }

        // 分页查询方法
        public IEnumerable<T> GetPage<T>(IEnumerable<T> collection, int pageNumber, int pageSize)
        {
            int startIndex = (pageNumber - 1) * pageSize;
            return collection.Skip(startIndex).Take(pageSize);
        }

        private static List<string> Recursion(List<string[]> list, int start, List<string> result)
        {
            if (start >= list.Count)
                return result;

            if (result.Count == 0)
                result = list[start].ToList();
            else
                result = result.SelectMany(x => list[start].Select(y => y + x)).ToList();
            result = (from a in result
                      from b in list[start]
                      select a + b).ToList();
            result = Recursion(list, start + 1, result);
            return result;
        }
    }

    // 使用 Except 取差集，对于复合类型需要实现 IEquatable<T> 接口
    internal class ExceptUser : IEquatable<ExceptUser>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool Equals(ExceptUser other)
        {
            return other != null && this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    internal class SelectManyDepartment
    {
        public SelectManyEmployee[] SelectManyEmployees { get; set; }
    }

    internal class SelectManyEmployee
    {
        public string Name { get; set; }
    }

    internal interface IFoo
    {
    }

    internal class Foo : IFoo
    {
    }

    internal class Bar : IFoo
    {
    }

    internal class IdEqualityComparer : IEqualityComparer<Person>
    {
        public bool Equals(Person x, Person y) => x?.Id == y?.Id;

        public int GetHashCode(Person obj) => obj.GetHashCode();
    }

    internal class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    internal class User
    {
        public int UserID { get; set; }
        public string Email { get; set; }
    }

    internal class Order
    {
        public int OrderID { get; set; }
        public string OrderTitle { get; set; }
        public int UserID { get; set; }
    }
}