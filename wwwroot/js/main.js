// wwwroot/js/main.js
(function ($) {
    "use strict";

    // Spinner
    (function spinner() {
        setTimeout(function () {
            if ($("#spinner").length > 0) $("#spinner").removeClass("show");
        }, 1);
    })();

    // wowjs
    new WOW().init();

    // Sticky Navbar
    $(window).on("scroll", function () {
        if ($(this).scrollTop() > 300) {
            $(".sticky-top").css("top", "0px");
        } else {
            $(".sticky-top").css("top", "-100px");
        }
    });

    // Dropdown on mouse hover (no 'active' added here)
    const $dropdown = $(".dropdown");
    const $dropdownToggle = $(".dropdown-toggle");
    const $dropdownMenu = $(".dropdown-menu");
    const showClass = "show";

    $(window).on("load resize", function () {
        if (this.matchMedia("(min-width: 992px)").matches) {
            $dropdown.hover(
                function () {
                    const $this = $(this);
                    $this.addClass(showClass);
                    $this.find($dropdownToggle).attr("aria-expanded", "true");
                    $this.find($dropdownMenu).addClass(showClass);
                },
                function () {
                    const $this = $(this);
                    $this.removeClass(showClass);
                    $this.find($dropdownToggle).attr("aria-expanded", "false");
                    $this.find($dropdownMenu).removeClass(showClass);
                }
            );
        } else {
            $dropdown.off("mouseenter mouseleave");
        }
    });

    // Back to top visibility
    $(window).on("scroll", function () {
        if ($(this).scrollTop() > 300) {
            $(".back-to-top").fadeIn("slow");
        } else {
            $(".back-to-top").fadeOut("slow");
        }
    });

    // ===== Active state helpers =====
    const navSelector = ".navbar .nav-link, .navbar .dropdown-item";
    const $navItems = $(navSelector);
    const DROPDOWN_SECTIONS = new Set(["booking", "team", "testimonial"]);

    function navHeight() {
        return document.querySelector(".navbar")?.offsetHeight || 0;
    }

    function clearActive() {
        $navItems.removeClass("active");
        $(".navbar .dropdown-toggle").removeClass("active");
    }

    function setActiveHref(href) {
        clearActive();
        // Activate exact match link(s)
        $navItems.filter(function () {
            const h = $(this).attr("href");
            return h && (h === href || (href === "#home" && (h === "#" || h === "#home")));
        }).addClass("active");

        // If a dropdown item is active, reflect it on the parent toggle
        const anyDropdownActive = $(".dropdown-menu .dropdown-item.active").length > 0;
        if (anyDropdownActive) {
            $(".navbar .dropdown-toggle").addClass("active");
        }
    }

    function setActiveById(id) {
        if (!id) return;
        setActiveHref("#" + id);
    }

    // ===== Smooth in-page scrolling under sticky navbar =====
    function scrollToEl(el) {
        const y = el.getBoundingClientRect().top + window.scrollY - navHeight();

        // Avoid CSS/JS conflict on section jumps
        const root = document.scrollingElement || document.documentElement;
        const prevBehavior = root.style.scrollBehavior;
        root.style.scrollBehavior = "auto";
        window.scrollTo({ top: y, behavior: "smooth" });
        requestAnimationFrame(() => {
            if (prevBehavior) root.style.scrollBehavior = prevBehavior;
            else root.style.removeProperty("scroll-behavior");
        });
    }

    function handleHash(h) {
        const id = (h || "#home").replace("#", "");
        const el = document.getElementById(id);
        if (el) {
            scrollToEl(el);
            setActiveById(id);
        }
    }

    // Clicks on anchors that start with '#', except the back-to-top button
    document.addEventListener("click", (e) => {
        const a = e.target.closest('a[href^="#"]:not(.back-to-top)');
        if (!a) return;

        const href = a.getAttribute("href");
        if (!href || href === "#") return;

        const id = href.slice(1);
        const target = document.getElementById(id);
        if (!target) return;

        e.preventDefault();
        scrollToEl(target);
        history.pushState(null, "", href);
        setActiveById(id);

        // collapse navbar on mobile after click
        const opened = document.querySelector(".navbar .collapse.show");
        if (opened && window.bootstrap?.Collapse) {
            new bootstrap.Collapse(opened, { toggle: true });
        }
    });

    // Back to top — then set Home active
    $(document).on("click", ".back-to-top", function (e) {
        e.preventDefault();
        const root = document.scrollingElement || document.documentElement;

        $(root).stop(true, false);

        const prevBehavior = root.style.scrollBehavior;
        root.style.scrollBehavior = "auto";

        $(root).animate({ scrollTop: 0 }, 600, function () {
            if (prevBehavior) root.style.scrollBehavior = prevBehavior;
            else root.style.removeProperty("scroll-behavior");

            setActiveHref("#home");
            history.replaceState(null, "", "#home");
        });
    });

    // Facts counter
    $("[data-toggle='counter-up']").counterUp({ delay: 10, time: 2000 });

    // Date & time picker
    $(".date").datetimepicker({ format: "L" });
    $(".time").datetimepicker({ format: "LT" });

    // Testimonials carousel
    $(".testimonial-carousel").owlCarousel({
        autoplay: true,
        smartSpeed: 1000,
        center: true,
        margin: 25,
        dots: true,
        loop: true,
        nav: false,
        responsive: { 0: { items: 1 }, 768: { items: 2 }, 992: { items: 3 } },
    });

    // ===== ScrollSpy-like behavior =====
    const sectionIds = ["home", "about", "services", "booking", "team", "testimonial"];
    const sections = sectionIds
        .map(id => document.getElementById(id))
        .filter(Boolean);

    function computeCurrentSection() {
        const offset = navHeight() + 10;
        const scrollPos = window.scrollY + offset;

        if (window.scrollY <= 5) return "home";

        let currentId = "home";
        for (const sec of sections) {
            if (sec.offsetTop <= scrollPos) {
                currentId = sec.id;
            }
        }
        return currentId;
    }

    function onScrollSetActive() {
        const id = computeCurrentSection();
        setActiveById(id);

        // Only light up "Pages" if one of its child sections is active.
        if (DROPDOWN_SECTIONS.has(id)) {
            $(".navbar .dropdown-toggle").addClass("active");
        } else {
            $(".navbar .dropdown-toggle").removeClass("active");
        }
    }

    window.addEventListener("scroll", onScrollSetActive, { passive: true });

    // ===== Initial load =====
    window.addEventListener("load", () => {
        // If there is no hash, force Home only; ensure Pages is NOT active.
        if (!location.hash) {
            setActiveHref("#home");              // activates Home, clears others
            $(".navbar .dropdown-toggle").removeClass("active"); // extra safety
        } else {
            handleHash(location.hash);           // respects deep links
        }
    });

    // Back/forward between hashes
    window.addEventListener("popstate", () => {
        if (location.hash) handleHash(location.hash);
        else {
            setActiveHref("#home");
            $(".navbar .dropdown-toggle").removeClass("active");
            const home = document.getElementById("home");
            if (home) scrollToEl(home);
        }
    });

})(jQuery);
