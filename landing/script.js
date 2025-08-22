/* ===================================
   DynaFetch Landing Page JavaScript
   Modern, efficient functionality with progressive enhancement
   =================================== */

(function () {
  'use strict';

  /* ===================================
       Utility Functions
       =================================== */

  /**
   * Debounce function to limit how often a function can be called
   * @param {Function} func - Function to debounce
   * @param {number} wait - Wait time in milliseconds
   * @param {boolean} immediate - Execute immediately on first call
   * @returns {Function} Debounced function
   */
  function debounce(func, wait, immediate) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        timeout = null;
        if (!immediate) func.apply(this, args);
      };
      const callNow = immediate && !timeout;
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
      if (callNow) func.apply(this, args);
    };
  }

  /**
   * Check if element is in viewport
   * @param {Element} element - Element to check
   * @returns {boolean} True if element is in viewport
   */
  function isInViewport(element) {
    const rect = element.getBoundingClientRect();
    return (
      rect.top >= 0 &&
      rect.left >= 0 &&
      rect.bottom <=
        (window.innerHeight || document.documentElement.clientHeight) &&
      rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
  }

  /**
   * Smooth scroll to target element
   * @param {string} targetSelector - CSS selector for target element
   * @param {number} offset - Offset from top in pixels
   */
  function smoothScrollTo(targetSelector, offset = 0) {
    const target = document.querySelector(targetSelector);
    if (!target) return;

    const targetPosition =
      target.getBoundingClientRect().top + window.pageYOffset - offset;

    window.scrollTo({
      top: targetPosition,
      behavior: 'smooth',
    });
  }

  /* ===================================
       Mobile Navigation
       =================================== */

  class MobileNavigation {
    constructor() {
      this.navToggle = document.querySelector('.nav-toggle');
      this.navMenu = document.querySelector('.nav-menu');
      this.navLinks = document.querySelectorAll('.nav-link');
      this.isOpen = false;

      this.init();
    }

    init() {
      if (!this.navToggle || !this.navMenu) return;

      // Toggle button click handler
      this.navToggle.addEventListener('click', () => this.toggleMenu());

      // Close menu when clicking nav links
      this.navLinks.forEach((link) => {
        link.addEventListener('click', () => this.closeMenu());
      });

      // Close menu when clicking outside
      document.addEventListener('click', (e) => {
        if (
          !this.navToggle.contains(e.target) &&
          !this.navMenu.contains(e.target)
        ) {
          this.closeMenu();
        }
      });

      // Close menu on escape key
      document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && this.isOpen) {
          this.closeMenu();
        }
      });

      // Handle window resize
      window.addEventListener(
        'resize',
        debounce(() => {
          if (window.innerWidth > 768 && this.isOpen) {
            this.closeMenu();
          }
        }, 250)
      );
    }

    toggleMenu() {
      if (this.isOpen) {
        this.closeMenu();
      } else {
        this.openMenu();
      }
    }

    openMenu() {
      this.isOpen = true;
      this.navToggle.classList.add('nav-toggle--active');
      this.navMenu.classList.add('nav-menu--active');

      // Prevent body scroll
      document.body.style.overflow = 'hidden';

      // Update ARIA attributes
      this.navToggle.setAttribute('aria-expanded', 'true');
    }

    closeMenu() {
      this.isOpen = false;
      this.navToggle.classList.remove('nav-toggle--active');
      this.navMenu.classList.remove('nav-menu--active');

      // Restore body scroll
      document.body.style.overflow = '';

      // Update ARIA attributes
      this.navToggle.setAttribute('aria-expanded', 'false');
    }
  }

  /* ===================================
       Header Scroll Effects
       =================================== */

  class HeaderScrollEffects {
    constructor() {
      this.header = document.querySelector('.header');
      this.lastScrollY = window.scrollY;
      this.scrollThreshold = 10;

      this.init();
    }

    init() {
      if (!this.header) return;

      window.addEventListener(
        'scroll',
        debounce(() => {
          this.handleScroll();
        }, 10)
      );
    }

    handleScroll() {
      const currentScrollY = window.scrollY;

      // Add scrolled class for styling
      if (currentScrollY > this.scrollThreshold) {
        this.header.classList.add('header--scrolled');
      } else {
        this.header.classList.remove('header--scrolled');
      }

      this.lastScrollY = currentScrollY;
    }
  }

  /* ===================================
       Smooth Scroll Navigation
       =================================== */

  class SmoothScrollNavigation {
    constructor() {
      this.headerHeight = 70; // Match CSS variable
      this.init();
    }

    init() {
      // Handle anchor links
      const anchorLinks = document.querySelectorAll('a[href^="#"]');

      anchorLinks.forEach((link) => {
        link.addEventListener('click', (e) => {
          const href = link.getAttribute('href');

          // Skip empty anchors
          if (href === '#') return;

          e.preventDefault();
          smoothScrollTo(href, this.headerHeight + 20);
        });
      });
    }
  }

  /* ===================================
       Copy to Clipboard Functionality
       =================================== */

  class CopyToClipboard {
    constructor() {
      this.init();
    }

    init() {
      // Add copy buttons to code blocks
      const codeBlocks = document.querySelectorAll('.code-block');

      codeBlocks.forEach((codeBlock) => {
        this.addCopyButton(codeBlock);
      });
    }

    addCopyButton(codeBlock) {
      const wrapper = document.createElement('div');
      wrapper.className = 'code-wrapper';

      const copyButton = document.createElement('button');
      copyButton.className = 'copy-button';
      copyButton.innerHTML = 'Copy';
      copyButton.setAttribute('aria-label', 'Copy code to clipboard');

      // Wrap code block
      codeBlock.parentNode.insertBefore(wrapper, codeBlock);
      wrapper.appendChild(codeBlock);
      wrapper.appendChild(copyButton);

      // Add click handler
      copyButton.addEventListener('click', () => {
        this.copyText(codeBlock.textContent.trim(), copyButton);
      });
    }

    async copyText(text, button) {
      try {
        if (navigator.clipboard && window.isSecureContext) {
          // Modern clipboard API
          await navigator.clipboard.writeText(text);
        } else {
          // Fallback for older browsers
          this.fallbackCopyText(text);
        }

        this.showCopySuccess(button);
      } catch (error) {
        console.warn('Copy failed:', error);
        this.showCopyError(button);
      }
    }

    fallbackCopyText(text) {
      const textArea = document.createElement('textarea');
      textArea.value = text;
      textArea.style.position = 'fixed';
      textArea.style.left = '-999999px';
      textArea.style.top = '-999999px';

      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();

      try {
        document.execCommand('copy');
      } finally {
        document.body.removeChild(textArea);
      }
    }

    showCopySuccess(button) {
      const originalText = button.innerHTML;
      button.innerHTML = 'Copied!';
      button.classList.add('copy-button--success');

      setTimeout(() => {
        button.innerHTML = originalText;
        button.classList.remove('copy-button--success');
      }, 2000);
    }

    showCopyError(button) {
      const originalText = button.innerHTML;
      button.innerHTML = 'Error';
      button.classList.add('copy-button--error');

      setTimeout(() => {
        button.innerHTML = originalText;
        button.classList.remove('copy-button--error');
      }, 2000);
    }
  }

  /* ===================================
       Scroll Animation (Intersection Observer)
       =================================== */

  class ScrollAnimations {
    constructor() {
      this.animatedElements = document.querySelectorAll('[data-animate]');
      this.init();
    }

    init() {
      if (!('IntersectionObserver' in window)) {
        // Fallback for older browsers - show all elements
        this.animatedElements.forEach((el) => {
          el.classList.add('animate-visible');
        });
        return;
      }

      const observer = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting) {
              entry.target.classList.add('animate-visible');
              observer.unobserve(entry.target);
            }
          });
        },
        {
          threshold: 0.1,
          rootMargin: '0px 0px -50px 0px',
        }
      );

      this.animatedElements.forEach((el) => {
        observer.observe(el);
      });
    }
  }

  /* ===================================
       External Link Handling
       =================================== */

  class ExternalLinks {
    constructor() {
      this.init();
    }

    init() {
      const externalLinks = document.querySelectorAll('a[target="_blank"]');

      externalLinks.forEach((link) => {
        // Add security attributes
        if (!link.hasAttribute('rel')) {
          link.setAttribute('rel', 'noopener noreferrer');
        }

        // Add visual indicator if not already present
        if (
          !link.classList.contains('nav-link--external') &&
          !link.querySelector('.external-icon')
        ) {
          this.addExternalIcon(link);
        }
      });
    }

    addExternalIcon(link) {
      const icon = document.createElement('span');
      icon.className = 'external-icon';
      icon.innerHTML = '↗';
      icon.setAttribute('aria-hidden', 'true');
      link.appendChild(icon);
    }
  }

  /* ===================================
       Performance Monitoring
       =================================== */

  class PerformanceMonitoring {
    constructor() {
      this.init();
    }

    init() {
      // Monitor page load performance
      if ('performance' in window) {
        window.addEventListener('load', () => {
          setTimeout(() => {
            this.logPerformanceMetrics();
          }, 0);
        });
      }
    }

    logPerformanceMetrics() {
      try {
        const navigation = performance.getEntriesByType('navigation')[0];
        if (navigation) {
          const loadTime = navigation.loadEventEnd - navigation.loadEventStart;
          const domContentLoaded =
            navigation.domContentLoadedEventEnd -
            navigation.domContentLoadedEventStart;

          // Only log in development (you can remove this in production)
          if (
            window.location.hostname === 'localhost' ||
            window.location.hostname === '127.0.0.1'
          ) {
            console.log('Page Performance:', {
              'Load Time': `${loadTime}ms`,
              'DOM Content Loaded': `${domContentLoaded}ms`,
              'Total Load Time': `${
                navigation.loadEventEnd - navigation.fetchStart
              }ms`,
            });
          }
        }
      } catch (error) {
        // Silently fail - performance monitoring shouldn't break the site
      }
    }
  }

  /* ===================================
       Initialization
       =================================== */

  function initializeApp() {
    // Initialize all components
    new MobileNavigation();
    new HeaderScrollEffects();
    new SmoothScrollNavigation();
    new CopyToClipboard();
    new ScrollAnimations();
    new ExternalLinks();
    new PerformanceMonitoring();

    // Add loaded class to body for CSS transitions
    document.body.classList.add('js-loaded');
  }

  /* ===================================
       DOM Ready and Load Events
       =================================== */

  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeApp);
  } else {
    initializeApp();
  }

  // Additional CSS for JavaScript-enhanced features
  const additionalStyles = `
        <style>
            /* Mobile Navigation Styles */
            @media (max-width: 768px) {
                .nav-menu {
                    position: fixed;
                    top: var(--header-height);
                    left: 0;
                    right: 0;
                    background-color: white;
                    flex-direction: column;
                    padding: var(--spacing-xl);
                    box-shadow: var(--shadow-lg);
                    transform: translateY(-100%);
                    opacity: 0;
                    visibility: hidden;
                    transition: all var(--transition-base);
                    z-index: 999;
                }
                
                .nav-menu--active {
                    transform: translateY(0);
                    opacity: 1;
                    visibility: visible;
                }
                
                .nav-toggle--active .hamburger {
                    background-color: transparent;
                }
                
                .nav-toggle--active .hamburger::before {
                    transform: rotate(45deg);
                    top: 0;
                }
                
                .nav-toggle--active .hamburger::after {
                    transform: rotate(-45deg);
                    bottom: 0;
                }
            }
            
            /* Header Scroll Effects */
            .header--scrolled {
                background-color: rgba(255, 255, 255, 0.98);
                box-shadow: var(--shadow-base);
            }
            
            /* Copy Button Styles */
            .code-wrapper {
                position: relative;
            }
            
            .copy-button {
                position: absolute;
                top: var(--spacing-sm);
                right: var(--spacing-sm);
                background-color: var(--color-gray-700);
                color: white;
                border: none;
                padding: var(--spacing-xs) var(--spacing-sm);
                border-radius: var(--radius-sm);
                font-size: var(--font-size-xs);
                cursor: pointer;
                transition: all var(--transition-fast);
                z-index: 10;
            }
            
            .copy-button:hover {
                background-color: var(--color-primary);
            }
            
            .copy-button--success {
                background-color: var(--color-success) !important;
            }
            
            .copy-button--error {
                background-color: #e74c3c !important;
            }
            
            /* External Link Icons */
            .external-icon {
                margin-left: var(--spacing-xs);
                font-size: 0.85em;
                opacity: 0.7;
            }
            
            /* Animation Classes */
            [data-animate] {
                opacity: 0;
                transform: translateY(20px);
                transition: all 0.6s ease;
            }
            
            [data-animate].animate-visible {
                opacity: 1;
                transform: translateY(0);
            }
            
            /* JavaScript-enabled body class */
            .js-loaded [data-animate] {
                transition: all 0.6s ease;
            }
        </style>
    `;

  // Inject additional styles
  document.head.insertAdjacentHTML('beforeend', additionalStyles);
})();
