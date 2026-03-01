using CloneProm.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CloneProm.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            try
            {
                var ctx = provider.GetRequiredService<ClonePromDbContext>();
                ctx.Database.Migrate();

                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

                // ===========================
                // 1. Категорії
                // ===========================
                string[] catNames = { "Electronics", "Home Appliances", "Books", "Clothing", "Toys" };
                foreach (var name in catNames)
                {
                    if (!ctx.Categories.Any(c => c.Name == name))
                        ctx.Categories.Add(new Category { Name = name });
                }
                ctx.SaveChanges();

                var electronics = ctx.Categories.First(c => c.Name == "Electronics");
                var appliances = ctx.Categories.First(c => c.Name == "Home Appliances");
                var books = ctx.Categories.First(c => c.Name == "Books");
                var clothing = ctx.Categories.First(c => c.Name == "Clothing");
                var toys = ctx.Categories.First(c => c.Name == "Toys");

                // ===========================
                // 2. Ролі
                // ===========================
                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { "Admin", "Seller", "User" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // ===========================
                // 3. Admin + Seller
                // ===========================
                string adminEmail = "admin@cloneprom.com";
                string adminPassword = "Admin123!";
                ApplicationUser adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
                    await userManager.CreateAsync(adminUser, adminPassword);
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                var adminSeller = ctx.Sellers.FirstOrDefault(s => s.UserId == adminUser.Id);
                if (adminSeller == null)
                {
                    adminSeller = new Seller
                    {
                        UserId = adminUser.Id,
                        ShopName = "Admin Shop",
                        Description = "Seeded admin seller"
                    };
                    ctx.Sellers.Add(adminSeller);
                    ctx.SaveChanges();
                }

                // ===========================
                // 4. Test Seller
                // ===========================
                string testSellerEmail = "seller@cloneprom.com";
                ApplicationUser testSellerUser = await userManager.FindByEmailAsync(testSellerEmail);
                if (testSellerUser == null)
                {
                    testSellerUser = new ApplicationUser { UserName = testSellerEmail, Email = testSellerEmail };
                    await userManager.CreateAsync(testSellerUser, "Seller123!");
                    await userManager.AddToRoleAsync(testSellerUser, "Seller");
                }

                var testSeller = ctx.Sellers.FirstOrDefault(s => s.UserId == testSellerUser.Id);
                if (testSeller == null)
                {
                    testSeller = new Seller
                    {
                        UserId = testSellerUser.Id,
                        ShopName = "Test Seller Shop",
                        Description = "Seeded test seller"
                    };
                    ctx.Sellers.Add(testSeller);
                    ctx.SaveChanges();
                }

                // ===========================
                // 5. Функція додавання продукту
                // ===========================
                void AddIfMissing(Product product)
                {
                    if (!ctx.Products.Any(p => p.Name == product.Name && p.SellerId == product.SellerId))
                        ctx.Products.Add(product);
                }

                // ===========================
                // 6. Продукти Admin (6 шт)
                // ===========================
                AddIfMissing(new Product
                {
                    Name = "Ноутбук Apple MacBook Air 13.6 M4 8GPU Sky Blue (MC6T4)",
                    Description = "Apple MacBook Air  — це черговий крок у розвитку однієї з найпопулярніших лінійок ноутбуків у світі. Ця модель зберігає знайомий легкий і тонкий дизайн своїх попередників, але отримала суттєве оновлення завдяки новому процесору M4, який значно підвищує продуктивність і відкриває нові можливості для роботи зі штучним інтелектом. \r\n\r\nЯскравий і чіткий дисплей  \r\nНоутбук пропонує 13,6-дюймовий IPS-дисплей, який забезпечує роздільну здатність 2560x1664. Завдяки цьому зображення матиме чіткий і деталізований вигляд, а широка колірна гама сприятиме відтворенню живих та реалістичних відтінків. Дисплей підтримує технологію True Tone, яка автоматично адаптує кольори до умов освітлення, роблячи перегляд комфортнішим для очей. \r\n\r\nПотужність і продуктивність  \r\nСерцем цього ноутбука є чип M4, який об’єднує центральний та графічний процесор разом з Neural Engine на одному кристалі, забезпечуючи разючу швидкість роботи. Завдяки підтримці апаратного прискорення трасування променів графіка стала ще більш плавною, що оцінять любителі ігор і творчі професіонали. Достатній обсяг оперативної пам’яті ідеально підходить для багатозадачності, а швидкий SSD-накопичувач забезпечує миттєвий доступ до файлів і програм.\r\n\r\nApple Intelligence і macOS Sequoia  \r\nMacBook Air працює під управлінням macOS Sequoia, найсучаснішої операційної системи від Apple, яка інтегрує функції Apple Intelligence. Ця система штучного інтелекту допомагає у написанні текстів, редагуванні зображень і виконанні складних завдань із максимальною ефективністю, зберігаючи при цьому конфіденційність даних. Оновлення включають функцію iPhone Mirroring, зручне розміщення вікон і вдосконалений Safari, що робить роботу ще продуктивнішою. Цей ноутбук готовий до майбутнього, пропонуючи інноваційні інструменти для творчості та повсякденного використання.\r\n\r\nАвтономність і портативність  \r\nMacBook Air 13.6 пропонує до 18 годин автономної роботи від одного заряду, що робить його надійним супутником під час довгих робочих днів або ж подорожей. Технологія швидкої зарядки дозволяє поповнити батарею до 50% лише за 30 хвилин за допомогою кабелю MagSafe, який легко приєднується завдяки магнітному кріпленню.\r\n\r\nПідключення та звук  \r\nНоутбук оснащений двома портами Thunderbolt 4, які забезпечують швидке підключення аксесуарів і зарядку, а також роз’ємом для навушників. Технологія Wi-Fi 6E гарантує вдвічі більшу пропускну здатність, що означає стабільне і швидке з’єднання з мережею. Аудіосистема підтримує просторовий звук із динамічним відстеженням рухів голови при використанні AirPods, створюючи ефект занурення. Чотири динаміки гарантуватимуть чистий і об’ємний звук, ідеальний як для роботи, так і для розваг.\r\n\r\nРобота з текстом і взаємодія\r\nКлавіатура Magic Keyboard із підсвіткою та Touch ID забезпечує комфортне введення тексту та швидкий доступ до системи, а Force Touch трекпад пропонує точне керування з підтримкою жестів. Загалом, цей ноутбук вдало поєднує стиль, функціональність і турботу про довкілля. ",
                    Price = 49999.99m,
                    Quantity = 10,
                    ImagePath = "/images/products/notebook_alpha.jpg",
                    CategoryId = electronics.Id,
                    SellerId = adminSeller.Id,
                    Rating = 4.5,
                    ReviewsCount = 12
                });

                AddIfMissing(new Product
                {
                    Name = "Миша Logitech G102 Lightsync White (910-005824)",
                    Description = "Logitech G102 Lightsync - ігрова дротова мишка відомого бренду. Підтримує підсвітку Lightsync, яка здатна прикрасити пристрій, візуалізувати звук, синхронізувати світло з діями на екрані.\r\n\r\nЄ гнучкою в налаштуваннях, оснащена високоточним датчиком, який забезпечує додаткову перевагу гравця над своїми суперниками. Прицільний вогонь, швидкісні атаки та чіткі злагоджені дії будуть доступні завдяки одній лише мишці.\r\n\r\nФірмове програмне забезпечення Logitech G HUB допоможе налаштувати мишку під свої потреби, призначити команди, контролювати систему. \r\n\r\nКлавіші мають спеціальний механізм, який забезпечує швидкий і чіткий відгук. А 5 рівнів чутливості датчика стануть в пригоді, коли гравець має або чітко прицілюватися, або максимально швидко атакувати.\r\n\r\n\r\n\r\nМаксимальний комфорт при використанні \r\nLogitech G102 має традиційну конструкцію з 6 кнопками, що забезпечує максимальну продуктивність та комфорт під час використання. За допомогою Logitech G HUB ви можете керувати процесом управління та виконувати налаштування ігрових клавіш. \r\n\r\nВисока точність та стабільність роботи\r\nКлючові кнопки миші використовують металевий пружинний механізм Logitech G, що гарантує високу точність та стабільну реакцію. Ці кнопки надійно працюють навіть після мільйона натискань, роблячи її відмінним варіантом для інтенсивної гри. \r\n\r\n\r\n\r\nНалаштування чутливості \r\nЗавдяки вбудованому датчику ігрового класу, мишка приємно вражає своєю чутливістю. Функція налаштування дозволяє змінювати чутливість в діапазоні від 200 до 8000 точок на дюйм, і 5 налаштувань дозволяють швидко перемикатися між різними параметрами. \r\n\r\n \r\n\r\nЯскраве RGB підсвічування \r\nМишка також оснащена RGB підсвічуванням за технологією LIGHTSYNC. Оберіть готові анімації або створіть свій власний стиль з 16.8 млн відтінків. Налаштуйте параметри та синхронізуйте їх з екраном, дозволяючи мишці змінювати колір у відповідь на відтворення фільмів чи гру.  ",
                    Price = 24.50m,
                    Quantity = 50,
                    ImagePath = "/images/products/mouse_mini.jpg",
                    CategoryId = electronics.Id,
                    SellerId = adminSeller.Id,
                    Rating = 4.0,
                    ReviewsCount = 8
                });

                AddIfMissing(new Product
                {
                    Name = "Клавіатура Razer BlackWidow V3 Green Switch Roblox Edition (RZ03-03542800-R3M1)",
                    Description = "Razer BlackWidow V3 - механічна ігрова клавіатура оснащена новітніми функціями та перемикачами RAZER GREEN. Безшумні перемикачі забезпечують надзвичайно плавне натискання клавіш без тактильного відгуку і включають звукоізоляцію, щоб ще більше зменшити і без того низький профіль.\r\n\r\nОсобливості пристрою:\r\n\r\n- механічні перемикачі; \r\n\r\n- підсвітка із мільйонами відтінків та гнучкою системою налаштування; \r\n\r\n- повністю прозорий дизайн перемикачів;\r\n\r\n- м'яка шкіряна підставка для максимального комфорту під час тривалої гри;\r\n\r\n- міцна алюмінієва конструкція.\r\n\r\nRazer BlackWidow V3 - клавіатура призначена для максимального занурення у світ геймінгу.",
                    Price = 9999.99m,
                    Quantity = 20,
                    ImagePath = "/images/products/keyboard_x.jpg",
                    CategoryId = electronics.Id,
                    SellerId = adminSeller.Id,
                    Rating = 4.8,
                    ReviewsCount = 20
                });

                AddIfMissing(new Product
                {
                    Name = "2100001 ORGANIC COTTON JERSEY",
                    Description = "Зручна бавовняна футболка.",
                    Price = 14.99m,
                    Quantity = 200,
                    ImagePath = "/images/products/classic_tshirt.jpg",
                    CategoryId = clothing.Id,
                    SellerId = adminSeller.Id,
                    Rating = 4.2,
                    ReviewsCount = 15
                });

                AddIfMissing(new Product
                {
                    Name = "3100040 DENSE NYLON-TC HAND SPRAYED REFLECTIVE",
                    Description = "Штани для зносу з компактного нейлону, вручну оброблені світловідбивним розчином, що створює мармуровий вигляд, мають еластичний пояс зі стрічкою. Виріб пофарбований засобом, що запобігає краплям.",
                    Price = 49.99m,
                    Quantity = 80,
                    ImagePath = "/images/products/denim_jeans.jpg",
                    CategoryId = clothing.Id,
                    SellerId = adminSeller.Id,
                    Rating = 4.3,
                    ReviewsCount = 9
                });

                AddIfMissing(new Product
                {
                    Name = "C# in Depth",
                    Description = "Практичний посібник з C#.",
                    Price = 39.99m,
                    Quantity = 100,
                    ImagePath = "/images/products/csharp_in_depth.jpg",
                    CategoryId = books.Id,
                    SellerId = adminSeller.Id,
                    Rating = 5.0,
                    ReviewsCount = 30
                });

                // ===========================
                // 7. Продукти Test Seller (6 шт)
                // ===========================
                AddIfMissing(new Product
                {
                    Name = "Пароочисник Karcher SC 4 Deluxe White (1.513-460.0)",
                    Description = "Kärcher SC 4 Deluxe — потужний пароочисник, що працює при тиску до 4.0 бар, здатний ефективно знищувати до 99.999% вірусів та 99.99% усіх побутових бактерій на твердих поверхнях. Це робить його ідеальним для гігієнічного очищення різних поверхонь у вашому домі, включаючи кухню, ванну кімнату та інші.\r\n\r\nТриступінчасте регулювання витрати пари дозволяє легко адаптувати потік пари до різних типів поверхонь і ступеня забруднення, що робить пристрій дуже гнучким і універсальним у використанні. Кнопка вкл/викл забезпечує зручне керування пристроєм.\r\n\r\nСвітлодіодний індикатор на корпусі пароочисника дозволяє завжди бути в курсі його стану. Червоний колір свідчить про нагрівання, а зелений – про готовність до роботи, що робить управління пристроєм простим і інтуїтивно зрозумілим\r\n\r\nПароочисник оснащений насадкою для підлоги EasyFix з гнучким шарніром, що гарантує максимальну ергономіку при використанні. Це дозволяє ефективно прибирати на всіх типах твердих підлог завдяки інноваційній пластинчастій технології. Система фіксації серветок із застібкою-липучкою дозволяє легко закріпити серветки та замінювати їх без контакту з брудом, що робить процес очищення гігієнічнішим і зручнішим.\r\n\r\nОдна з основних переваг цього пароочисника — знімний бак для води, який дає можливість постійного дозаправлення. Це дозволяє працювати без необхідності робити довгі перерви на наповнення резервуара, що значно покращує зручність використання та ефективність роботи.\r\n\r\nПароочисник Kärcher SC 4 Deluxe також комплектується багатофункціональними аксесуарами, такими як ручна насадка, кругла щітка та інші приладдя, що дозволяють ефективно очищати різноманітні поверхні, включаючи плитку, варильні панелі та витяжки. Серветки та обтяжки з поліпшеними властивостями відділення і поглинання бруду забезпечують максимальний рівень чистоти.\r\n\r\nВбудований великий відсік для зберігання аксесуарів гарантує зручне місце для організації всіх необхідних елементів, таких як кабелі та шланги. Також є можливість швидко закріпити насадку для підлоги на час перерв у роботі, що полегшує зберігання та економить місце.\r\n\r\nДля безпеки, пароочисник оснащений системою блокування на пістолеті, яка забезпечує надійний захист від неправильного використання дітьми.\r\n\r\nKärcher SC 4 Deluxe — це ідеальне рішення для тих, хто хоче поєднати високий рівень гігієни з комфортом і зручністю у догляді за своїм домом.",
                    Price = 13999m,
                    Quantity = 25,
                    ImagePath = "/images/products/air_purifier.jpg",
                    CategoryId = appliances.Id,
                    SellerId = testSeller.Id,
                    Rating = 4.1,
                    ReviewsCount = 7
                });

                AddIfMissing(new Product
                {
                    Name = "Блендер Gorenje HBX1000E",
                    Description = "Gorenje HBX1000E — це поєднання потужності, функціональності та ергономічного дизайну, яке зробить процес приготування ще приємнішим і швидшим. Він стане ідеальним помічником для приготування різноманітних страв. Завдяки широкому вибору насадок ви зможете легко змішувати, збивати, подрібнювати й пюрирувати інгредієнти для супів, соусів, смузі та інших страв. Міцна конструкція з нержавіючої сталі гарантує довговічність, а ергономічний дизайн забезпечує комфорт у використанні.\r\n\r\nЦей блендер оснащений потужним і водночас енергоефективним мотором, який забезпечує стабільну роботу та довгий термін служби. Завдяки безшумній роботі ви зможете готувати улюблені страви, не турбуючи домашніх.\r\n\r\nБлендер Gorenje HBX1000E має кілька рівнів швидкості, що дозволяє адаптувати його роботу під різні продукти. Для зручного керування передбачено розумну круглу ручку, яка дозволяє змінювати швидкість одним рухом пальця.\r\n\r\nДля миттєвого збільшення потужності передбачена турбофункція. Просто натисніть на м’яку ергономічну ручку — і блендер працюватиме на максимальній швидкості, забезпечуючи ідеальний результат за лічені секунди.\r\n\r\nБлендер оснащений інноваційним лезом із нержавіючої сталі, яке швидко та ретельно подрібнює продукти. Завдяки спеціальній формі леза забезпечується рівномірне змішування без грудочок.\r\n\r\nУ комплекті з блендером ви знайдете кілька корисних аксесуарів. Основна насадка для змішування дозволяє швидко створювати однорідні пюре та кремові супи. Заміна насадок відбувається одним рухом — наприклад, щоб збити яйця або вершки, достатньо під’єднати віночок. Також у комплект входить подрібнювач із чашею об’ємом 500 мл, який ефективно подрібнює горіхи, сир, зелень та інші тверді продукти.\r\n\r\nПісля використання достатньо зняти насадку та промити її під проточною водою. Компактний дизайн дозволяє зручно зберігати блендер навіть у невеликих кухонних шафах.",
                    Price = 1999m,
                    Quantity = 40,
                    ImagePath = "/images/products/blender_500w.jpg",
                    CategoryId = appliances.Id,
                    SellerId = testSeller.Id,
                    Rating = 4.6,
                    ReviewsCount = 11
                });

                AddIfMissing(new Product
                {
                    Name = "Конструктор LEGO Technic Ferrari Daytona SP3 (42143)",
                    Description = "Вдумливе конструювання з пристрастю Ferrari\r\nДодайте наснаги креативності, збираючи культову модель суперкара LEGO® Technic Ferrari Daytona SP3 в масштабі 1:8.",
                    Price = 5499m,
                    Quantity = 150,
                    ImagePath = "/images/products/blocks_set.jpg",
                    CategoryId = toys.Id,
                    SellerId = testSeller.Id,
                    Rating = 4.7,
                    ReviewsCount = 18
                });

                AddIfMissing(new Product
                {
                    Name = "Автомодель Країна Іграшок на радіокеруванні Поліцейська машина салатова (998-6B/1)",
                    Description = "Автомодель Країна Іграшок на радіокеруванні Поліцейська машина — справжній хіт для маленьких любителів техніки та пригод! Завдяки реалістичному дизайну, світловим ефектам і можливості керування, вона подарує дітям безліч захоплюючих годин гри.",
                    Price = 529m,
                    Quantity = 70,
                    ImagePath = "/images/products/remote_car.jpg",
                    CategoryId = toys.Id,
                    SellerId = testSeller.Id,
                    Rating = 4.3,
                    ReviewsCount = 14
                });

                AddIfMissing(new Product
                {
                    Name = "Cooking Basics",
                    Description = "Книга рецептів для початківців.",
                    Price = 19.50m,
                    Quantity = 60,
                    ImagePath = "/images/products/cooking_basics.jpg",
                    CategoryId = books.Id,
                    SellerId = testSeller.Id,
                    Rating = 4.0,
                    ReviewsCount = 6
                });

                AddIfMissing(new Product
                {
                    Name = "Смарт годинник Apple Watch Ultra 3 - 49mm Black Titanium Case with Black Titanium Milanese Loop - Medium (MF1Q4)",
                    Description = "Apple Watch Ultra 3 — це потужний смартгодинник, створений для тих, хто прагне поєднати активний спосіб життя, пригоди та сучасні технології. З міцним дизайном і передовими функціями він стане ідеальним супутником для спорту, подорожей і щоденного використання.\r\n\r\nСтиль і зручність\r\nApple Watch Ultra 3 має корпус із титану. Цей матеріал забезпечує легкість і надзвичайну міцність, що ідеально підходить для екстремальних умов. Завдяки ширококутному дисплею Retina, який завжди увімкнений і має яскравість до 3000 ніт, інформація залишається чіткою навіть під прямим сонячним світлом.\r\n\r\nСпорт і активність\r\nЦей годинник створений для спортсменів і любителів активного відпочинку. Він оснащений точним двочастотним GPS, який забезпечує надійне відстеження маршрутів під час бігу, велоспорту чи піших прогулянок. Функція Workout Buddy і Custom Workouts допомагають оптимізувати тренування, а під час плавання годинник автоматично визначає стиль і відстежує дистанцію. Для велосипедистів Ultra 3 показує швидкість і ефективність у реальному часі, а для дайверів пропонує повноцінний дайв-комп’ютер із метриками глибини.\r\n\r\nЗдоров’я та безпека\r\nApple Watch Ultra 3 піклується про ваше здоров’я завдяки новітнім функціям. Унікальна функція повідомлень про високий кров’яний тиск аналізує дані за 30-денний період і попереджає про можливі проблеми. Годинник також пропонує ЕКГ, моніторинг серцевого ритму, відстеження рівня кисню в крові, оцінку якості сну та повідомлення про апное сну. У разі надзвичайної ситуації функція Emergency SOS через супутник дозволяє швидко зв’язатися з екстреними службами, навіть якщо ви далеко від мережі.\r\n\r\nАвтономність і зв’язок\r\nЗавдяки покращеному акумулятору Apple Watch Ultra 3 працює до 42 годин у звичайному режимі або до 72 годин у режимі низького енергоспоживання. Швидка зарядка забезпечує до 12 годин роботи лише за 15 хвилин підзарядки. Годинник підтримує 5G, що дозволяє залишатися на зв’язку під час пробіжок, а супутниковий зв’язок забезпечує спілкування через Messages і функцію Find My навіть у віддалених місцях.\r\n\r\nУніверсальність і додатки\r\nApple Watch Ultra 3 легко адаптується до будь-якої ситуації – від ділових зустрічей до вечері чи тренувань. Ви можете відповідати на дзвінки, надсилати повідомлення, слухати музику чи оплачувати покупки завдяки NFC. У App Store доступні додатки, такі як Strava, Komoot чи Oceanic+, які розширюють можливості для бігу, катання на лижах чи дайвінгу. З Apple Watch Ultra 3 ви отримуєте максимум функціональності в одному пристрої.",
                    Price = 48999m,
                    Quantity = 30,
                    ImagePath = "/images/products/smartwatch_pro.jpg",
                    CategoryId = electronics.Id,
                    SellerId = testSeller.Id,
                    Rating = 4.4,
                    ReviewsCount = 10
                });

                ctx.SaveChanges();
                logger.LogInformation("SeedData completed: Admin + Test Seller with products created.");
            }
            catch (Exception ex)
            {
                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
                logger.LogError(ex, "Error seeding the database.");
            }
        }
    }
}