"""Independent candidate references: Python integer binomial tails and Decimal existing paired CI.

No Core, R, Excel, network, scipy or promoted numerical golden is used. The
normal quantile uses a Decimal normal-density antiderivative series/bisection;
matched odds limits invert finite binomial tails (Clopper-Pearson), independently
of Core's F quantiles. The paired difference CI is the documented existing
Wilson/correlation formula evaluated with widened Decimal arithmetic.
"""
from decimal import Decimal as D, localcontext
from fractions import Fraction
from functools import lru_cache
from math import comb
from pathlib import Path
import argparse
import csv
import hashlib
import json

ROOT=Path(__file__).resolve().parents[1]
CSV=ROOT/"BESHStat.Core.Tests/TestData/04A05Completion/paired-candidates-v1.csv"
META=ROOT/"docs/architecture/04A05/paired-candidate-reference-v1.json"
PI=D("3.1415926535897932384626433832795028841971693993751058209749445923")
CASES=[("canonical",100,20,15,40,".05"),("swap",100,15,20,40,".05"),
       ("alpha10",100,20,15,40,".1"),("alpha20",100,20,15,40,".2"),
       ("no-events",100,0,0,0,".05"),("no-discordance",100,0,0,40,".05"),
       ("all-both",100,0,0,100,".05"),("first-zero",10,0,3,2,".05"),
       ("second-zero",10,3,0,2,".05"),("equal-positive",10,1,1,3,".05"),
       ("equal20",100,20,20,20,".05"),("all-discordant",35,20,15,0,".05"),
       ("tiny-first",1,1,0,0,".05"),("tiny-second",1,0,1,0,".05"),
       ("tiny-neither",1,0,0,0,".05"),("tiny-both",1,0,0,1,".05"),
       ("overflow",1000,100,80,300,".05"),("neighbor",900,90,72,270,".05"),
       ("larger",1000000,100,80,300000,".05"),
       ("int32-total",2147483647,20,15,1000000000,".05"),
       ("extreme-tail",1000,1000,0,0,".05"),
       ("small-backward",100,10,20,30,".05")]


def normal_cdf(z):
    term=z
    total=term
    for k in range(1,1000):
        term*= -z*z/(2*k)
        contribution=term/(2*k+1)
        total+=contribution
        if abs(contribution)<D("1e-65"):break
    else:raise ArithmeticError("Normal series did not converge")
    return D(".5")+total/(2*PI).sqrt()


@lru_cache(None)
def zcrit(alpha):
    target=1-alpha/2
    lo,hi=D(0),D(8)
    for _ in range(210):
        mid=(lo+hi)/2
        if normal_cdf(mid)<target:lo=mid
        else:hi=mid
    return (lo+hi)/2


def paired_ci(n,b,c,a,alpha):
    n,b,c,a=map(D,(n,b,c,a))
    z=zcrit(alpha)
    p1,p2=(a+b)/n,(a+c)/n
    def wilson(r):
        center=2*r+z*z
        half=z*(z*z+4*r*(1-r/n)).sqrt()
        return (center-half)/(2*(n+z*z)),(center+half)/(2*(n+z*z))
    l1,u1=wilson(a+b)
    l2,u2=wilson(a+c)
    if p1 in (0,1) or p2 in (0,1):phi=D(0)
    else:
        product=(a+b)*(n-a-b)*(a+c)*(n-a-c)
        cross=a*(n-a-b-c)-b*c
        adjusted=cross-n/2 if cross>n/2 else D(0) if cross>=0 else cross
        phi=adjusted/product.sqrt()
    low_rad=(p1-l1)**2-2*phi*(p1-l1)*(u2-p2)+(u2-p2)**2
    up_rad=(p2-l2)**2-2*phi*(p2-l2)*(u1-p1)+(u1-p1)**2
    assert low_rad>=0 and up_rad>=0
    estimate=p1-p2
    return estimate,estimate-low_rad.sqrt(),estimate+up_rad.sqrt(),phi,z


def exact_p(b,c):
    n=b+c
    if n==0:return Fraction(1)
    return min(Fraction(1),Fraction(2*sum(comb(n,k) for k in range(min(b,c)+1)),2**n))


def binomial_sum(n,ks,p):
    return sum((D(comb(n,k))*p**k*(1-p)**(n-k) for k in ks),D(0))


def odds_limits(b,c,alpha):
    n=b+c
    if n==0:return D("NaN"),D("NaN")
    target=alpha/2
    if b==0:lower=D(0)
    elif c==0:
        p=target**(D(1)/n)
        lower=p/(1-p)
    else:
        lo,hi=D(0),D(1)
        for _ in range(210):
            mid=(lo+hi)/2
            if binomial_sum(n,range(b,n+1),mid)<target:lo=mid
            else:hi=mid
        p=(lo+hi)/2
        lower=p/(1-p)
    if c==0:upper=D("Infinity")
    elif b==0:
        p=1-target**(D(1)/n)
        upper=p/(1-p)
    else:
        lo,hi=D(0),D(1)
        for _ in range(210):
            mid=(lo+hi)/2
            if binomial_sum(n,range(b+1),mid)>target:lo=mid
            else:hi=mid
        p=(lo+hi)/2
        upper=p/(1-p)
    return lower,upper


def rows():
    with localcontext() as context:
        context.prec=65
        result=[]
        for name,n,b,c,a,text_alpha in CASES:
            alpha=D(text_alpha)
            estimate,lower,upper,phi,z=paired_ci(n,b,c,a,alpha)
            assert -1-D("1e-50")<=lower<=upper<=1+D("1e-50")
            p=exact_p(b,c)
            ol,ou=odds_limits(b,c,alpha)
            result.append(dict(case=name,n=n,first_only=b,second_only=c,both=a,alpha=str(alpha),estimate=str(estimate),
                lower=str(lower),upper=str(upper),exact_p=str(D(p.numerator)/D(p.denominator)),or_lower=str(ol),or_upper=str(ou),phi=str(phi),z=str(z)))
        return result


def create():
    data=rows()
    CSV.parent.mkdir(parents=True,exist_ok=True)
    with CSV.open("x",encoding="utf-8",newline="") as stream:
        writer=csv.DictWriter(stream,fieldnames=list(data[0]))
        writer.writeheader();writer.writerows(data)
    META.parent.mkdir(parents=True,exist_ok=True)
    with META.open("x",encoding="utf-8",newline="\n") as stream:
        json.dump(dict(schema="04A05-completion-independent-candidates-v1",status="CANDIDATE_NOT_P_GOLDEN",
            independent_scientific_reviewers=0,source="Python standard library integer combinations/Fraction; Decimal65 density series+bisection and existing documented paired CI formula",
            generator_sha256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),fixture=CSV.relative_to(ROOT).as_posix(),fixture_sha256=hashlib.sha256(CSV.read_bytes()).hexdigest(),
            cases=len(data),p_tolerance="max(1e-315, min(2e-12, abs(reference)*2e-8))",estimate_absolute_tolerance=5e-15,
            ci_absolute_tolerance=2e-9,ci_relative_tolerance=2e-8,odds_limit_absolute_tolerance=2e-7,odds_limit_relative_tolerance=2e-7,
            formula_policy="Same existing Wilson/correlation/continuity rule. No clamp, pseudocount, changed CI method or changed Core critical-value algorithm.",
            limitations="Candidate numerical verification, not two-person scientific approval, native testing, R qualification or global golden promotion."),stream,indent=2)
        stream.write("\n")
    for row in data:
        if row["case"] in ("canonical","overflow","equal-positive","extreme-tail"):print(row)


def check():
    with CSV.open(encoding="utf-8",newline="") as stream:actual=list(csv.DictReader(stream))
    expected=[{k:str(v) for k,v in r.items()} for r in rows()]
    assert actual==expected,"Candidate fixture differs from independent generator"
    meta=json.loads(META.read_bytes())
    assert meta["fixture_sha256"]==hashlib.sha256(CSV.read_bytes()).hexdigest()
    assert meta["generator_sha256"]==hashlib.sha256(Path(__file__).read_bytes()).hexdigest()
    print("Independent candidates reproduce:",len(actual),"cases; no Core or native execution")


if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action",choices=["create","check"])
    action=parser.parse_args().action
    create() if action=="create" else check()
