"use client";

import { useState } from "react";
import {
  Activity, ArrowUpLeft, Bell, Bot, CalendarDays, Check, ChevronDown,
  ChevronLeft, Clapperboard, Clock3, Facebook, FileText, Filter, Instagram,
  LayoutDashboard, MessageCircle, MoreHorizontal, Play, Plus, Search,
  Settings2, Sparkles, Workflow, Youtube, Zap
} from "lucide-react";

type View = "overview" | "calendar" | "studio" | "automation" | "inbox";

const navigation: { id: View; label: string; icon: typeof LayoutDashboard }[] = [
  { id: "overview", label: "نمای کلی", icon: LayoutDashboard },
  { id: "calendar", label: "تقویم محتوا", icon: CalendarDays },
  { id: "studio", label: "استودیو ویدئو", icon: Clapperboard },
  { id: "automation", label: "اتوماسیون هوشمند", icon: Workflow },
  { id: "inbox", label: "صندوق تعامل", icon: MessageCircle },
];

const metrics = [
  { label: "دسترسی کل", value: "۱۲۸.۴ هزار", change: "+۱۸.۶٪", tone: "cyan", bars: [22, 35, 27, 48, 39, 58, 43, 65, 52, 75, 58, 82] },
  { label: "تعاملات", value: "۲۴,۸۶۰", change: "+۱۲.۲٪", tone: "violet", bars: [30, 43, 34, 55, 45, 67, 50, 72, 62, 80, 68, 91] },
  { label: "رشد دنبال‌کننده", value: "۳,۲۴۸", change: "+۸.۴٪", tone: "green", bars: [18, 29, 25, 39, 33, 49, 42, 59, 52, 68, 61, 79] },
  { label: "نرخ پاسخ‌گویی", value: "۹۶.۸٪", change: "+۴.۱٪", tone: "orange", bars: [35, 43, 39, 52, 48, 60, 55, 67, 62, 76, 70, 86] },
];

const days = [
  { day: "شنبه", date: "۱۲", posts: [{ network: "اینستاگرام", title: "معرفی محصول جدید", time: "۱۰:۳۰", tone: "pink" }, { network: "استوری", title: "پشت صحنه برند", time: "۱۸:۰۰", tone: "purple" }] },
  { day: "یکشنبه", date: "۱۳", posts: [{ network: "اینستاگرام", title: "نکته آموزشی هفته", time: "۱۲:۰۰", tone: "pink" }] },
  { day: "دوشنبه", date: "۱۴", posts: [{ network: "تلگرام", title: "خبرنامه هفتگی", time: "۰۹:۰۰", tone: "blue" }, { network: "ریل", title: "ویدئوی معرفی", time: "۲۰:۰۰", tone: "purple" }] },
  { day: "سه‌شنبه", date: "۱۵", posts: [{ network: "اینستاگرام", title: "داستان مشتری", time: "۱۱:۳۰", tone: "pink" }] },
  { day: "چهارشنبه", date: "۱۶", posts: [{ network: "اینستاگرام", title: "پیشنهاد ویژه", time: "۱۷:۰۰", tone: "pink" }] },
  { day: "پنجشنبه", date: "۱۷", posts: [{ network: "استوری", title: "نظرسنجی تعاملی", time: "۱۹:۳۰", tone: "purple" }] },
  { day: "جمعه", date: "۱۸", posts: [] },
];

function MetricCard({ item }: { item: (typeof metrics)[number] }) {
  return <article className="metric-card">
    <div className="metric-top"><span>{item.label}</span><span className="metric-icon"><Activity size={17} /></span></div>
    <div className="metric-bottom"><strong>{item.value}</strong><span className="metric-change"><ArrowUpLeft size={13} />{item.change}</span></div>
    <div className={`sparkline ${item.tone}`}>{item.bars.map((height, index) => <i key={index} style={{ height: `${height}%` }} />)}</div>
    <small>نسبت به ۳۰ روز گذشته</small>
  </article>;
}

export default function HomePage() {
  const [active, setActive] = useState<View>("overview");
  const [period, setPeriod] = useState("۷ روز اخیر");
  const current = navigation.find((item) => item.id === active)?.label ?? "نمای کلی";

  return <main className="app-shell">
    <aside className="sidebar">
      <a className="brand" href="#" onClick={(event) => { event.preventDefault(); setActive("overview"); }}>
        <span className="brand-mark"><Zap size={22} fill="currentColor" /></span>
        <span>اینستا<span className="brand-light">پایلوت</span><small>AI WORKSPACE</small></span>
      </a>
      <div className="workspace-switch"><div className="workspace-avatar">ن</div><div><b>نوآوران دیجیتال</b><small>فضای کاری اصلی</small></div><ChevronDown size={16} /></div>
      <p className="nav-caption">مدیریت و انتشار</p>
      <nav className="side-nav">{navigation.map(({ id, label, icon: Icon }) => <button key={id} className={active === id ? "nav-item active" : "nav-item"} onClick={() => setActive(id)}><Icon size={18} /><span>{label}</span>{id === "inbox" && <em>۸</em>}</button>)}</nav>
      <p className="nav-caption nav-caption-spaced">هوش مصنوعی</p>
      <button className="nav-item" onClick={() => setActive("automation")}><Bot size={18} /><span>دستیار هوشمند</span><span className="new-pill">جدید</span></button>
      <button className="nav-item" onClick={() => setActive("automation")}><Workflow size={18} /><span>گردش‌کارها</span></button>
      <div className="sidebar-bottom">
        <div className="plan-card"><div className="plan-icon"><Sparkles size={17} /></div><b>پلن حرفه‌ای</b><p>از تمام قابلیت‌های اینستا پایلوت استفاده کنید.</p><div className="plan-progress"><i /></div><small>۷۲٪ از اعتبار ماهانه</small><button onClick={() => setActive("overview")}>مدیریت اشتراک <ChevronLeft size={14} /></button></div>
        <button className="nav-item"><Settings2 size={18} /><span>تنظیمات</span></button>
        <div className="profile"><div className="profile-avatar">ا</div><div><b>آرمان رضایی</b><small>مدیر فضای کاری</small></div><MoreHorizontal size={18} /></div>
      </div>
    </aside>

    <section className="main-area">
      <header className="topbar">
        <div className="breadcrumbs"><span>فضای کاری</span><ChevronLeft size={14} /><b>{current}</b></div>
        <div className="top-actions"><label className="search-box"><Search size={16} /><input placeholder="جستجو در اینستا پایلوت..." /></label><button className="icon-button" aria-label="اعلان‌ها"><Bell size={18} /><i /></button><div className="top-divider" /><span className="top-date">پنج‌شنبه، ۲۶ آبان ۱۴۰۵</span><div className="mini-avatar">ا</div></div>
      </header>

      <div className="page-content">
        <section className="welcome-row">
          <div><div className="eyebrow"><span /> پنل مدیریت هوشمند</div><h1>{active === "overview" ? "سلام آرمان، خوش اومدی 👋" : current}</h1><p>{active === "overview" ? "عملکرد کسب‌وکارت رو بررسی کن و برای رشد بعدی آماده شو." : "همه ابزارهای موردنیاز برای مدیریت حرفه‌ای شبکه‌های اجتماعی."}</p></div>
          <div className="welcome-actions"><button className="button-secondary"><CalendarDays size={16} /> این ماه <ChevronDown size={14} /></button><button className="button-primary" onClick={() => setActive("studio")}><Plus size={17} /> ایجاد محتوای جدید</button></div>
        </section>

        <section className="metric-grid">{metrics.map((item) => <MetricCard key={item.label} item={item} />)}</section>

        <section className="insight-grid">
          <article className="growth-card">
            <div className="section-heading"><div><span className="section-kicker">ANALYTICS OVERVIEW</span><h2>روند رشد و تعامل</h2><p>نمای کلی عملکرد صفحه در بازه انتخابی</p></div><button className="select-button" onClick={() => setPeriod(period === "۷ روز اخیر" ? "۳۰ روز اخیر" : "۷ روز اخیر")}>{period}<ChevronDown size={14} /></button></div>
            <div className="chart-legend"><span><i className="legend-dot violet-dot" /> تعاملات</span><span><i className="legend-dot cyan-dot" /> دسترسی</span></div>
            <div className="chart-area"><div className="chart-y"><span>۱۵k</span><span>۱۰k</span><span>۵k</span><span>۰</span></div><div className="chart-plot"><div className="chart-gridlines"><i /><i /><i /><i /></div><svg viewBox="0 0 680 190" preserveAspectRatio="none" role="img" aria-label="نمودار رشد تعاملات و دسترسی"><defs><linearGradient id="violetFill" x1="0" x2="0" y1="0" y2="1"><stop offset="0%" stopColor="#9b5cff" stopOpacity=".28" /><stop offset="100%" stopColor="#9b5cff" stopOpacity="0" /></linearGradient></defs><path d="M0 145 C45 135 55 105 105 118 S165 145 205 92 S270 105 310 78 S365 112 415 62 S485 85 520 52 S600 75 680 20 L680 190 L0 190Z" fill="url(#violetFill)" /><path d="M0 145 C45 135 55 105 105 118 S165 145 205 92 S270 105 310 78 S365 112 415 62 S485 85 520 52 S600 75 680 20" fill="none" stroke="#a66bff" strokeWidth="3" /><path d="M0 165 C50 155 75 145 115 150 S180 120 220 135 S285 115 330 128 S395 95 445 110 S510 85 550 95 S625 65 680 75" fill="none" stroke="#25c5d8" strokeWidth="2.5" strokeLinecap="round" /></svg><div className="chart-x"><span>۱ آبان</span><span>۵ آبان</span><span>۱۰ آبان</span><span>۱۵ آبان</span><span>۲۰ آبان</span><span>۲۶ آبان</span></div></div></div>
          </article>
          <article className="platform-card">
            <div className="section-heading"><div><span className="section-kicker">CONNECTED CHANNELS</span><h2>کانال‌های متصل</h2><p>وضعیت حساب‌های اجتماعی شما</p></div><button className="dots-button"><MoreHorizontal size={19} /></button></div>
            <div className="channel-row"><span className="channel-icon instagram"><Instagram size={19} /></span><div className="channel-info"><b>اینستاگرام</b><small>@noavaran.digital</small></div><span className="connected"><i /> متصل</span></div>
            <div className="channel-row"><span className="channel-icon telegram">➤</span><div className="channel-info"><b>تلگرام</b><small>@noavaran_channel</small></div><span className="connected"><i /> متصل</span></div>
            <div className="channel-row"><span className="channel-icon youtube"><Youtube size={19} /></span><div className="channel-info"><b>یوتیوب</b><small>هنوز متصل نشده</small></div><button className="connect-link">اتصال</button></div>
            <button className="add-channel"><Plus size={16} /> افزودن کانال جدید</button>
          </article>
        </section>

        <section className="lower-grid">
          <article className="calendar-card">
            <div className="section-heading"><div><span className="section-kicker">CONTENT PLANNER</span><h2>برنامه محتوایی این هفته</h2><p>۱۲ تا ۱۸ آبان ۱۴۰۵</p></div><button className="text-link" onClick={() => setActive("calendar")}>مشاهده تقویم <ArrowUpLeft size={15} /></button></div>
            <div className="week-grid">{days.map((day) => <div className="day-column" key={day.date}><div className="day-heading"><span>{day.day}</span><b>{day.date}</b></div><div className="day-posts">{day.posts.map((post) => <div className={`post-chip ${post.tone}`} key={post.title}><span>{post.network}</span><b>{post.title}</b><small><Clock3 size={11} /> {post.time}</small></div>)}{day.posts.length === 0 && <span className="empty-day">بدون محتوا</span>}</div></div>)}</div>
          </article>
          <article className="assistant-card"><div className="assistant-glow" /><div className="assistant-heading"><span className="assistant-icon"><Sparkles size={18} /></span><div><span className="section-kicker">INSTAPILOT AI</span><h2>دستیار هوشمند شما</h2></div><span className="online-dot" /></div><p>عملکرد این هفته‌ات رو بررسی کردم. سه پیشنهاد برای افزایش تعامل آماده کردم.</p><div className="suggestion"><span className="suggestion-icon"><Zap size={15} /></span><span><b>زمان انتشار بهینه</b><small>امروز ساعت ۲۰:۳۰ بهترین زمان انتشار ریلز است.</small></span><ChevronLeft size={15} /></div><div className="suggestion"><span className="suggestion-icon cyan"><Activity size={15} /></span><span><b>ایده محتوای ترند</b><small>یک ویدئوی آموزشی کوتاه می‌تواند تعامل را افزایش دهد.</small></span><ChevronLeft size={15} /></div><button className="assistant-cta" onClick={() => setActive("automation")}>گفت‌وگو با دستیار <ArrowUpLeft size={15} /></button></article>
        </section>
        <footer className="footer-note"><span>© ۱۴۰۵ اینستا پایلوت</span><span><i /> همه سرویس‌ها فعال هستند</span><span>ساخته‌شده برای رشد هوشمند کسب‌وکار شما</span></footer>
      </div>
    </section>
  </main>;
}
