import { Link as RouterLink } from "react-router-dom";

interface LinkProps {
  href: string;
  children: React.ReactNode;
  className?: string;
}

export default function Link({
  href,
  children,
  className = "",
  ...props
}: LinkProps): React.ReactElement {
  const isInternal = href.startsWith("/");

  if (isInternal) {
    return (
      <RouterLink
        to={href}
        className={`text-slate-400 hover:text-teal-300 ${className}`}
        {...props}
      >
        {children}
      </RouterLink>
    );
  }

  return (
    <a
      href={href}
      className={`text-slate-400 hover:text-teal-300 ${className}`}
      {...props}
    >
      {children}
    </a>
  );
}
