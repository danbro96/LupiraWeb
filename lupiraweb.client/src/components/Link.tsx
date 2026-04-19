import NextLink from "next/link";

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
  const classes = `text-slate-400 hover:text-teal-300 ${className}`;

  if (isInternal) {
    return (
      <NextLink href={href} className={classes} {...props}>
        {children}
      </NextLink>
    );
  }

  return (
    <a href={href} className={classes} {...props}>
      {children}
    </a>
  );
}
