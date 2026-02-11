import type { ReactNode } from "react";

interface SectionCardProps {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
  children: ReactNode;
}

export default function SectionCard({ title, subtitle, actions, children }: SectionCardProps) {
  return (
    <section className="card">
      <header className="card-header">
        <div>
          <h2>{title}</h2>
          {subtitle ? <p className="card-subtitle">{subtitle}</p> : null}
        </div>
        {actions ? <div className="card-actions">{actions}</div> : null}
      </header>
      <div className="card-body">{children}</div>
    </section>
  );
}
